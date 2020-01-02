module Program
open Types
open Decision
open Sanctions
open Hunt
open Build
open Config
open Duma
open Opinion
open System.IO
open Agent
open CSVDump

[<EntryPoint>]
let main argv =
    // Agent parsing - test with command line args "--number-days -1 --number-profiles 7 --number-agents 24"
    let agents = Parsing.parse argv

    let printAgent (agent : Agent) =
        ("ID: ", agent.ID), ("Energy: ", agent.Energy), ("Susceptibility: ", agent.Susceptibility),
        ("Idealism: ", agent.Idealism), ("Egotism: ", agent.Egotism),
        ("Gain: ", agent.Gain), ("EnergyConsumed: ", agent.EnergyConsumed), ("EnergyDeprecation: ", agent.EnergyDeprecation),
        ("HuntedFood: ", agent.HuntedFood), ("Activity: ", agent.TodaysActivity), ("ShelterAccess: ", agent.AccessToShelter),
        ("SelfConfidence: ", agent.SelfConfidence), ("LastCrimeDate: ", agent.LastCrimeDate), ("FoodAccess: ", agent.AccessToFood),
        ("Alive: ", agent.Alive), ("OverallRuleOpinion: ", agent.DecisionOpinions.Value.OverallRuleOpinion),
        ("OtherAgentsOpinion: ", List.map (fun (ag, opin) -> ag.ID, opin) agent.DecisionOpinions.Value.AllOtherAgentsOpinion)

    let printWorld (world : WorldState) =
        (("Buildings: ", world.Buildings), ("Time to new chair: ", world.TimeToNewChair), ("CurrentRules: ", world.CurrentRuleSet),
         ("CurrentDay: ", world.CurrentDay), ("CurrentChair: ", world.CurrentChair.Value.ID), ("NumHare: ", world.NumHare), ("NumStag: ", world.NumStag),
         ("AllRules: ", world.AllRules))
        
    let currentWorld =
        {
            Buildings = List.Empty;
            TimeToNewChair = 5;
            CurrentShelterRule = Random;
            CurrentFoodRule = Communism;
            CurrentVotingRule = Borda;
            CurrentWorkRule = Everyone;
            CurrentMaxPunishment = Exile;
            CurrentSanctionStepSize = 0.1;
            CurrentDay = 0;
            CurrentChair = None;
            NumHare = 15;
            NumStag = 15;
            CurrentRuleSet = initialiseRuleSet;
            AllRules = initialiseAllRules;
            BuildingRewardPerDay = 0.0;
            HuntingRewardPerDay = 0.0;
            BuildingAverageTotalReward = 0.0;
            HuntingAverageTotalReward = 0.0;
            S = [0.5; 0.5; 0.5];
            ShuntingEnergySplit = List.init 11 (fun _ -> 0.5);
        }

    let updateEndOfDayState (agents: Agent list) (state: WorldState) : WorldState = 
        let updateNoneAgentStates (currentWorld: WorldState) : WorldState =
            {currentWorld with CurrentDay = currentWorld.CurrentDay + 1;
                                NumHare = currentWorld.NumHare + regenRate rabbosMeanRegenRate currentWorld.NumHare maxNumHare;
                                NumStag = currentWorld.NumStag + regenRate staggiMeanRegenRate currentWorld.NumStag maxNumStag}  // Regeneration
        
        let updateSocialGoodForWork (state: WorldState) : WorldState =
            let socialGood (prevVal: float) (activity: Activity) : float = 
                let targets = 
                    agents
                    |> List.filter (fun el -> fst el.TodaysActivity = activity)
                if targets = [] then prevVal    // If no agent carries out action, social good not changed
                else
                    targets
                    |> List.map (fun el -> el.Gain - 2.0 * el.EnergyConsumed - 2.0 * el.EnergyDeprecation)
                    |> List.sum
                    |> fun x -> x / (agents.Length |> float)

            let updatedS =
                List.zip [NONE; HUNTING; BUILDING] state.S
                |> List.map (fun (work, s) ->
                    work
                    |> socialGood s
                    |> getCumulativeAverage state.CurrentDay s
                )
                |> standardize

            {state with S = updatedS}
            
        let updateSocialGoodForHunters (state: WorldState) : WorldState =
            let socialGood (prevVal: float) (option: int) : float = 
                let targets = 
                    agents
                    |> List.filter (fun el -> el.TodaysHuntOption = option)
                
                if targets = [] then prevVal
                else
                    targets
                    |> List.map (fun el -> el.Gain - 2.0 * el.EnergyConsumed - 2.0 * el.EnergyDeprecation)
                    |> List.sum
                    |> fun x -> x / (agents.Length |> float)

            let updatedS =
                List.zip [for i in 0..10 -> i] state.ShuntingEnergySplit
                |> List.map (fun (option, s) ->
                    option
                    |> socialGood s
                    |> getCumulativeAverage state.CurrentDay s
                )
                |> standardize

            {state with ShuntingEnergySplit = updatedS}
        
        state
        |> updateAverageTotalRewards agents
        |> updateSocialGoodForWork
        |> updateSocialGoodForHunters
        |> updateNoneAgentStates

    let rec loop (currentWorld : WorldState) (agents : Agent list) (writer : StreamWriter) (csvwriter : StreamWriter) : WorldState =
        let livingAgents = agents |> List.filter (fun el -> el.Alive = true)
        let deadAgents = agents |> List.filter (fun el -> el.Alive = false)

        // Duma session
        let currentWorld = fullDuma livingAgents currentWorld
        
        // Work allocation
        let agentsWithJobs =
            livingAgents
            |> List.map (fun el -> {el with FoodShared = false}) // Reset foodsharingn status for agents
            |> List.map (fun el ->
                let decision = workAllocation el currentWorld // To verify
                match decision with
                | 0 -> {el with TodaysActivity = NONE, 1.0}
                | 1 -> 
                    let huntingStrategy = huntStrategyDecision el currentWorld
                    {el with TodaysActivity = HUNTING, 1.0;
                                TodaysHuntOption = huntingStrategy}
                | 2 -> {el with TodaysActivity = BUILDING, 1.0}
                | _ -> failwith("Invalid decision")
            )
            
        let builders =
            agentsWithJobs
            |> List.filter (fun el -> fst el.TodaysActivity = BUILDING)

        // Update shelter
        let currentWorld = newWorldShelters currentWorld builders

        let slackers =
            agentsWithJobs
            |> List.filter (fun el -> fst el.TodaysActivity = NONE)

        let hunters, currentWorld = 
            agentsWithJobs
            |> List.filter (fun el -> fst el.TodaysActivity = HUNTING)
            |> hunt currentWorld


        // Food energy for allocation
        let energyForAllocation = 
            hunters
            // Discounts agents who do not share food without sanctioning them
            |> List.map (fun el -> el.HuntedFood - el.Gain)
            |> List.sum
            
        // Re-concatenate the individually processed groups
        let agentsAfterWorking = hunters @ builders @ slackers

        let idealEnergyAssignment, idealWorkStatus = idealAllocation currentWorld agentsAfterWorking energyForAllocation

        // Resource Allocation
        let agentsAfterResorceAllocation =
            agentsAfterWorking
            |> allocateFood idealEnergyAssignment
            |> assignShelters currentWorld
        // Sanction
            |> detectCrime currentWorld idealEnergyAssignment idealWorkStatus
            |> sanction currentWorld
        // Energy decay due to working
            |> reduceEnergyForWorking
        // End-of-turn energy decay
            |> List.map (fun el -> newAgentEnergy el)
        // End-of-turn infamy decay
            |> infamyDecay currentWorld
        // Update reward matrices for works
            |> updateWorkRewardMatrices

        // Opinion, Payoff, Social Good updates
        let opinionChangesAgents = agentsAfterResorceAllocation
                                     |> updateRuleOpinion
                                     |> updateRewardsForEveryRuleForAgent currentWorld
        let currentWorld = normaliseTheSocialGood (updateSocialGoodForEveryCurrentRule  opinionChangesAgents currentWorld)
        
        let normalisedAgentArrays = normaliseTheAgentArrays opinionChangesAgents
                                     |> updateAggregationArrayForAgent currentWorld
                                     |> workOpinions currentWorld
                                     |> selfConfidenceUpdate
            
        // After sanction, agent may die
        let deadAgentsAfterToday = 
            deadAgents @ (normalisedAgentArrays |> List.filter (fun el -> el.Alive = false || el.Energy <= 0.0))
            |> List.map (fun agent -> {agent with Alive = false})
        let livingAgentsAfterToday = 
            normalisedAgentArrays 
            |> List.filter (fun el -> el.Alive = true && el.Energy > 0.0)

        let currentWorld = updateEndOfDayState agents currentWorld

        writer.Write ("Living Agents in day ")
        writer.Write (currentWorld.CurrentDay)
        List.map (fun agent -> writer.Write (printAgent agent)) livingAgentsAfterToday |> ignore
        writer.WriteLine ()
        writer.WriteLine ()
        writer.Write("World Status in day ")
        writer.Write (currentWorld.CurrentDay)
        writer.Write (printWorld currentWorld)
        writer.WriteLine ()
        writer.WriteLine ()
        writer.Write("END OF DAY ")
        writer.Write (currentWorld.CurrentDay)

        if livingAgentsAfterToday.Length = 0 || currentWorld.CurrentDay = maxSimulationTurn then
            csvdump currentWorld (livingAgentsAfterToday @ deadAgentsAfterToday) csvwriter
        else
            printfn "Living Agents: %A" (printAgent (List.head livingAgentsAfterToday))
            //printfn "Current world status: %A" (printWorld currentWorld)
            printfn "End of DAY: %A" currentWorld.CurrentDay
            writer.WriteLine ()
            writer.WriteLine ()
            loop (csvdump currentWorld (livingAgentsAfterToday @ deadAgentsAfterToday) csvwriter)
                 (livingAgentsAfterToday @ deadAgentsAfterToday)
                  writer 
                  csvwriter

    // csv file headings
    let headings = "CurrentDay,Buildings,CurrentChair,TimeToNewChair,CurrentShelterRule,CurrentVotingRule,CurrentFoodRule,CurrentWorkRule,CurrentMaxPunishment,CurrentSanctionStepSize,NumHare,NumStag,BuildingRewardPerDay,HuntingRewardPerDay,BuildingAverageTotalReward,HuntingAverageTotalReward,"
    
    // agent headings duplicated for each agent
    let agentHeadings = "[ID]Susceptibility,[ID]Idealism,[ID]Egotism,[ID]Gain,[ID]EnergyDepreciation,[ID]EnergyConsumed,[ID]Infamy,[ID]Energy,[ID]HuntedFood,[ID]Today'sActivity,[ID]AccessToShelter,[ID]SelfConfidence,[ID]Today'sHuntOption,[ID]FoodSharing,[ID]LastCrimeDate,[ID]AccessToFood,[ID]Alive,"


    let writer = new StreamWriter("..\..\..\output.txt")
    let csvwriter = new System.IO.StreamWriter("..\..\..\test.csv")
    printfn "headings: %s" headings
    csvwriter.Write(headings)
    csvwriter.Write(List.fold (fun acc elem -> acc + agentHeadings.Replace("[ID]",string elem.ID)) "" agents)
    let finalWorld = loop currentWorld agents writer csvwriter
    printfn "Final world status: %A" (printWorld finalWorld);
    printfn "Last day %A" finalWorld.CurrentDay
    writer.Write("Final world: ")
    writer.WriteLine(finalWorld)
    writer.Close()
    csvwriter.Close()

    0
