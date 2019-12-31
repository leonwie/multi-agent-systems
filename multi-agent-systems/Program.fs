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
open System.IO
open System.IO

[<EntryPoint>]
let main argv =
    // Agent parsing - test with command line args "--number-days -1 --number-profiles 7 --number-agents 24"
    let agents = Parsing.parse argv

    let printAgents (agents : Agent list) =
        List.map (fun agent -> (("ID: ", agent.ID), ("Energy: ", agent.Energy), ("Susceptibility: ", agent.Susceptibility),
                                ("Idealism: ", agent.Idealism), ("Egotism: ", agent.Egotism),
                                ("Gain: ", agent.Gain), ("EnergyConsumed: ", agent.EnergyConsumed), ("EnergyDeprecation: ", agent.EnergyDeprecation),
                                ("HuntedFood: ", agent.HuntedFood), ("Activity: ", agent.TodaysActivity), ("ShelterAccess: ", agent.AccessToShelter),
                                ("SelfConfidence: ", agent.SelfConfidence), ("LastCrimeDate: ", agent.LastCrimeDate), ("FoodAccess: ", agent.AccessToFood),
                                ("Alive: ", agent.Alive), ("OverallRuleOpinion: ", agent.DecisionOpinions.Value.OverallRuleOpinion),
                                ("OtherAgentsOpinion: ", List.map (fun (ag, opin) -> ag.ID, opin) agent.DecisionOpinions.Value.AllOtherAgentsOpinion))) agents

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
        }


    let rec loop (currentWorld : WorldState) (agents : Agent list) (writer : StreamWriter) : WorldState=
        let livingAgents = agents |> List.filter (fun el -> el.Alive = true)
        let deadAgents = agents |> List.filter (fun el -> el.Alive = false)

        // Duma session
        let currentWorld = fullDuma livingAgents currentWorld
        
        // Work allocation
        let agentsWithJobs =
            livingAgents
            |> List.map (fun el ->
                let decision, payoff = workAllocation el currentWorld // To verify
                match decision with
                | 0 -> {el with TodaysActivity = NONE, 1.0}
                | 1 -> {el with TodaysActivity = STAG, payoff}
                | 2 -> {el with TodaysActivity = HARE, payoff}
                | 3 -> {el with TodaysActivity = BUILDING, 1.0}
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

        let hareHunters =
            agentsWithJobs
            |> List.filter (fun el -> fst el.TodaysActivity = HARE)
            |> capHare
            |> shareFood currentWorld

        let stagHunters =
            agentsWithJobs
            |> List.filter (fun el -> fst el.TodaysActivity = STAG)
            |> capStag
            |> shareFood currentWorld

        // Food energy for allocation
        let energyForAllocation = 
            hareHunters @ stagHunters
            // Discounts agents who do not share food without sanctioning them
            |> List.map (fun el -> el.HuntedFood - el.Gain)
            |> List.sum
            
        // Re-concatenate the individually processed groups
        let agentsAfterWorking = hareHunters @ stagHunters @ builders @ slackers

        let idealEnergyAssignment, idealWorkStatus = idealAllocation currentWorld agentsAfterWorking energyForAllocation

        // Resource Allocation
        let agentsAfterResorceAllocation =
            agentsAfterWorking
            |> allocateFood idealEnergyAssignment
            |> assignShelters currentWorld
        // Sanction
            |> detectCrime currentWorld idealEnergyAssignment idealWorkStatus
            |> sanction currentWorld
        // End-of-turn energy decay
            |> List.map (fun el -> newAgentEnergy el)
        // End-of-turn infamy decay
            |> infamyDecay currentWorld

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
        let deadAgentsAfterToday = deadAgents @ (normalisedAgentArrays |> List.filter (fun el -> el.Alive = false || el.Energy <= 0.0))
        let livingAgentsAfterToday = 
            normalisedAgentArrays 
            |> List.filter (fun el -> el.Alive = true && el.Energy > 0.0)
        // Reset food captured field associated with current day
        // TODO if we need to record agent state for each day,
        // we must do it before reset here
            |> List.map (fun el ->
                {el with HuntedFood = 0.0; Gain = 0.0}
            )

        let currentWorld =
            {currentWorld with CurrentDay = currentWorld.CurrentDay + 1;
                                NumHare = currentWorld.NumHare + regenRate rabbosMeanRegenRate currentWorld.NumHare maxNumHare;
                                NumStag = currentWorld.NumStag + regenRate staggiMeanRegenRate currentWorld.NumStag maxNumStag}  // Regeneration

        writer.Write ("Living Agents in day ")
        writer.Write (currentWorld.CurrentDay)
        writer.WriteLine (printAgents livingAgentsAfterToday)
        writer.Write("World Status in day ")
        writer.Write (currentWorld.CurrentDay)
        writer.WriteLine (printWorld currentWorld)
        writer.Write("END OF DAY ")
        writer.Write (currentWorld.CurrentDay)

        //printfn "Living Agents: %A" (printAgents livingAgentsAfterToday)
        //printfn "Current world status: %A" (printWorld currentWorld)
        printfn "End of DAY: %A" currentWorld.CurrentDay
        writer.WriteLine ()
        writer.WriteLine ()

        if livingAgentsAfterToday.Length = 0 || currentWorld.CurrentDay = maxSimulationTurn then
            currentWorld
        else
            loop currentWorld (livingAgentsAfterToday @ deadAgentsAfterToday) writer

    let writer = new StreamWriter("..\..\..\output.txt")
    let finalWorld = loop currentWorld agents writer
    printfn "Final world status: %A" (printWorld finalWorld);
    printfn "Last day %A" finalWorld.CurrentDay
    writer.Write("Final world: ")
    writer.WriteLine(finalWorld)
    writer.Close

    0