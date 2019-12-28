module Program
open Agent
open Types
open Activities
open Sanctions
open Hunt
open Build
open Config

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
        if currentWorld.CurrentDay = maxSimulationTurn then currentWorld
        else
            let livingAgents = agents |> List.filter (fun el -> el.Alive = true)
            let deadAgents = agents |> List.filter (fun el -> el.Alive = false)

            let agentsWithJobs = 
                livingAgents 
                |> jobAllocation;

            let builders = 
                agentsWithJobs
                |> List.filter (fun el -> fst el.TodaysActivity = BUILDING)

            // Update shelter
            let currentWorld = newWorldShelters currentWorld builders

    

            let hunters =
                agentsWithJobs
                |> List.filter (fun el -> fst el.TodaysActivity = HUNTING)

            let hareCaptured = 
                hunters
                |> List.map (fun el -> 
                    rand.Next() 
                    |> float
                    |> capHare rabbosMinRequirement rabbosProbability)// PlaceHolder for agent decision
                |> List.sum

            let stagCaptured = 
                hunters
                |> List.map (fun el -> rand.Next() |> float) // PlaceHolder for agent decisions
                |> capStag staggiMinIndividual staggiMinCollective staggiProbability

            let energyGained = hareCaptured * rabbosEnergyValue + stagCaptured * staggiEnergyValue

            let idealEnergyAssignment, idealWorkStatus = idealAllocation currentWorld livingAgents energyGained

            // Resource Allocation
            let livingAgents =
                livingAgents
                |> allocateFood idealEnergyAssignment
                |> assignShelters currentWorld
            // Sanction 
                |> detectCrime currentWorld idealEnergyAssignment idealWorkStatus
                |> sanction currentWorld
            // End-of-turn energy decay
                |> List.map (fun el -> newAgentEnergy el)
            // End-of-turn infamy decay
                |> infamyDecay currentWorld



            // After sanction, agent may die
            let deadAgents = deadAgents @ (livingAgents |> List.filter (fun el -> el.Alive = false))
            let livingAgents = livingAgents |> List.filter (fun el -> el.Alive = true)

            let currentWorld = 
                {currentWorld with CurrentDay = currentWorld.CurrentDay + 1; 
                                    NumHare = currentWorld.NumHare + regenRate rabbosMeanRegenRate currentWorld.NumHare maxNumHare; 
                                    NumStag = currentWorld.NumStag + regenRate staggiMeanRegenRate currentWorld.NumStag maxNumStag}  // Regeneration

            // printfn "Dead Agents: %A" (agentsWithNewEnergy |> List.filter (fun el -> el.Energy <= 0.0))
            printfn "Living Agents: %A" (livingAgents |> List.filter (fun el -> el.Energy > 0.0))
            printfn "Current world status: %A" currentWorld
            
            if livingAgents.Length = 0 || currentWorld.CurrentDay = 20 then
                currentWorld
            else
                loop currentWorld (livingAgents @ deadAgents)
    
    let finalWorld = loop currentWorld agents;

    printfn "Final world status: %A" finalWorld;
    
    0