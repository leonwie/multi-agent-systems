
open System
open Agent
open Types
open Voting
open Activities
open Hunt
open Build
open Config

[<EntryPoint>]
let main argv =
    // Agent parsing - test with command line args "--number-days 20 --number-profiles 2"
    let agents = Parsing.parse argv
    
    // WARNING: Order matters
    let initialiseRuleSet = [(Shelter(Random), 0.5, 0.5); (Food(Communism), 0.5, 0.5); (Work(Everyone), 0.5, 0.5); (Voting(Borda), 0.5, 0.5); (Sanction(Exile), 0.5, 0.5)]
    
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
            RuleSet = initialiseRuleSet;
            GlobalSocialGood = 0.0;
            AverageSocialGood = 0.0;
        }

    
    let rec loop (currentWorld : WorldState) (agents : Agent list) : WorldState =
        if currentWorld.CurrentDay = maxSimulationTurn then currentWorld
        else

            let agentsWithJobs = 
                agents 
                |> jobAllocation;

            let builders = 
                agentsWithJobs
                |> List.filter (fun el -> fst el.TodaysActivity = BUILDING)

            // Update shelter
            let currentWorld = newWorldShelters currentWorld builders

            // Assign shelter
            let agents = assignShelters currentWorld agents

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

            // Resorce Allocation

            // To ensure consistency of agent definition, use oldAgents first
            let currentWorld = newWorldShelters currentWorld agentsWithJobs
            

            // event resolution
            let agentsWithNewEnergy = 
                agentsWithJobs
                |> assignShelters currentWorld
                |> List.map (fun el -> newAgentEnergy el)


            let currentWorld = 
                {currentWorld with CurrentDay = currentWorld.CurrentDay + 1; 
                                    NumHare = currentWorld.NumHare + regenRate rabbosMeanRegenRate currentWorld.NumHare maxNumHare; 
                                    NumStag = currentWorld.NumStag + regenRate staggiMeanRegenRate currentWorld.NumStag maxNumStag}  // Regeneration

            // printfn "Dead Agents: %A" (agentsWithNewEnergy |> List.filter (fun el -> el.Energy <= 0.0))
            printfn "Living Agents: %A" (agentsWithNewEnergy |> List.filter (fun el -> el.Energy > 0.0))
            printfn "Current world status: %A" currentWorld
            loop currentWorld agentsWithNewEnergy

    
    let finalWorld = loop currentWorld agents;

    printfn "Final world status: %A" finalWorld;
    
    0


//open multi_agent_systems

//[<EntryPoint>]
//let main argv =
//    // Config tested with arguments "--number-days 20 --number-profiles 2"
//    let agents = Parsing.parse argv

//    let whatToDo (agents : Agent list) =
//        agents
//        |> List.map (fun el ->
//            "\n" + (string)el.ID +
//            " will do " + (el.TodaysActivity |> fst |> string) +
//            " and expend " + (el.TodaysActivity |> snd |> string) + " energy."
//        )

//    let allAgents =
//        agents 
//        |> jobAllocation

//    let builders =
//        allAgents
//        |> List.filter (fun el -> fst el.TodaysActivity = BUILDING)

//    let hunters =
//        allAgents
//        |> List.filter (fun el -> fst el.TodaysActivity = HUNTING)
    
//    printfn "%A" agents
//    printfn "%A" (whatToDo allAgents)
    
//    0 // return an integer exit code
