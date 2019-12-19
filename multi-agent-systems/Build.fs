module Build

open Types
open Config
open WorldState

// Based on energy put in decide how many shelters to build 
let newWorldShelters (currentWorld : WorldState) (builders : Agent list) : WorldState =
    let energySpentBuildingShelter (builders : Agent list) : float =
        builders
        |> List.sumBy (fun el -> snd el.TodaysActivity)

    // How many new shelters are built
    let sheltersBuilt (shelterCost : int) (energySpent : float) : int =
        (energySpent |> int) / shelterCost // Integer division so no need for floor

    let newSheltersBuilt =
        builders
        |> energySpentBuildingShelter
        |> sheltersBuilt costOfBuilding     

    let shelterAfterDecay (curQuality : float) (numShelters : int) (qualityDecayRate : float) (numMaintainers : int) (maintainCost : float) (energyPerShelter : float) : float =
        numMaintainers 
        |> float
        |> (*) maintainCost 
        |> (/) (numShelters |> float) 
        |> (/) energyPerShelter
        |> floor
        |> (+) (curQuality * (1.0 - qualityDecayRate))
        |> min 1.0

    let newBuildings = 
        currentWorld.Buildings
        |> List.map (fun el -> shelterAfterDecay el (List.length currentWorld.Buildings) rg (List.length builders) em es) // Existing buildings decay
        |> List.filter (fun el -> el <> 0.0) // If shelter health at 0 then its gone
        |> List.append (List.init newSheltersBuilt (fun _ -> 1.0)) // All new shelters have 100 health
   
    {currentWorld with Buildings = newBuildings}


// Return all the agents with access to shelter field set
let assignShelters (currentWorld : WorldState) (agents : Agent list) : Agent list =
    let availableBuildings = 
        currentWorld.Buildings 

    let assignBuilding (agent : Agent) (shelter : float option) : Agent =
        {agent with AccessToShelter = shelter}

    // Need to arrange list depending on rule
    let agents = 
        match currentWorld.CurrentShelterRule with
        | Random -> 
            let shuffle l =
                let rand = new System.Random()
                let a = l |> Array.ofList
                let swap (a: _[]) x y =
                    let tmp = a.[x]
                    a.[x] <- a.[y]
                    a.[y] <- tmp
                Array.iteri (fun i _ -> swap a i (rand.Next(i, Array.length a))) a
                a |> Array.toList
            shuffle agents // List of agents is randomised
        | Socialism -> 
            agents // Weakest at front of list
            |> List.sortBy (fun el -> el.Energy) 
        | Meritocracy -> 
            agents // Those who put most energy in at front of list
            |> List.sortBy (fun el -> el.TodaysEnergyExpended) 
            |> List.rev
        | Oligarchy -> 
            agents // Strongest at front of list
            |> List.sortBy (fun el -> el.Energy) 
            |> List.rev

    // Assign Shelter
    agents
    |> List.fold (
        fun acc el ->
            match (acc |> snd) with
            | [] -> (fst acc) @ [assignBuilding el None], (snd acc) // No need for shelter if hasn't got access
            | h :: t -> (fst acc) @ [assignBuilding el (Some h)], t
        ) ([], availableBuildings)
    |> fst

let newAgentEnergy (agent : Agent) : Agent =
    match agent.AccessToShelter with
    | None -> {agent with Energy = agent.Energy - rb} // rb is base energy lost
    | Some(quality) -> {agent with Energy = agent.Energy - (1.0 - quality * ep) * rb} // ep is max shelter energy preservation