module Program
open Types
open Decision
open Sanctions
open Hunt
open Build
open Config
open Duma
open Opinion

[<EntryPoint>]
let main argv =
    // Agent parsing - test with command line args "--number-days 20 --number-profiles 2"
    let agents = Parsing.parse argv

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


    let rec loop (currentWorld : WorldState) (agents : Agent list) : WorldState =
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

        //printfn "Living Agents: %A" (List.map (fun ag -> (ag.ID, ag.Energy)) livingAgentsAfterToday)
        //printfn "Current world status: %A" currentWorld
        printfn "End of DAY: %A" currentWorld.CurrentDay

        if livingAgentsAfterToday.Length = 0 || currentWorld.CurrentDay = maxSimulationTurn then
            currentWorld
        else
            loop currentWorld (livingAgentsAfterToday @ deadAgentsAfterToday)

    let finalWorld = loop currentWorld agents;

    printfn "Final world status: %A" finalWorld;
    printfn "Last day %A" finalWorld.CurrentDay

    0