module Build

open Types
open Config

// Based on energy put in decide how many shelters to build 
let newWorldShelters (currentWorld : WorldState) (builders : Agent list) : WorldState =
    let energySpentBuildingShelter (builders : Agent list) : float =
        builders
        |> List.sumBy (fun el -> snd el.TodaysActivity)

    // How many new shelters are built
    let sheltersBuilt (shelterCost : int) (energySpent : float) : int =
        (energySpent |> int) / shelterCost

    let newSheltersBuilt =
        builders
        |> energySpentBuildingShelter
        |> sheltersBuilt costOfBuilding     
        
    let newBuildings = 
        currentWorld.Buildings
        |> List.map (fun el -> el - shelterDecayRate) // Existing buildings decay
        |> List.filter (fun el -> not (el = 0.0)) // If shelter health at 0 then its gone
        |> List.append (List.init newSheltersBuilt (fun _ -> 100.0)) // All new shelters have 100 health
   
    {currentWorld with Buildings = newBuildings}


// Return all the agents with access to shelter fielf set
let assignShelters (currentWorld : WorldState) (agents : Agent list) : Agent list =
    let availableBuildings = 
        currentWorld.Buildings 
        |> List.length

    let assignBuilding (agent : Agent) : Agent =
        {agent with AccessToShelter = true}

    // Sort so that the agents with the least energy have shelter and give them first access to buildings
    agents
    |> List.sortBy (fun el -> el.Energy)
    |> List.fold (
        fun acc el ->
            if (acc |> snd) > 0
            then ((fst acc) @ [assignBuilding el], (snd acc) - 1)
            else ((fst acc) @ [el], (snd acc))
        ) ([], availableBuildings)
    |> fst

