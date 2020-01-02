module Build

open Types
open Config

// Based on energy put in decide how many shelters to build 
let newWorldShelters (currentWorld : WorldState) (builders : Agent list) : WorldState =
    let energySpentBuildingShelter (builders : Agent list) : float =
        builders
        |> List.sumBy (fun el -> snd el.TodaysActivity)

    // How many new shelters are built
    let sheltersBuilt (shelterCost : float) (energySpent : float) : int =
        let multiplier = 10.0
        (energySpent * multiplier / shelterCost) 
        |> floor
        |> int 

    let newSheltersBuilt (builders : Agent list) =
        builders
        |> energySpentBuildingShelter
        |> sheltersBuilt eb     

    let shelterAfterDecay (curQuality : float) (numShelters : int) (qualityDecayRate : float) (numMaintainers : int) (maintainCost : float) (energyPerShelter : float) : float =
        (numMaintainers |> float) * (maintainCost / ((numShelters |> float) * energyPerShelter))
        //|> floor
        //|> (+) (curQuality * (1.0 - qualityDecayRate))
        |> (+) (curQuality - qualityDecayRate)
        |> min 1.0

    let newBuildings = 
        let numMaintainers =
            min (List.length builders / 2) (List.length currentWorld.Buildings)
        let newBuilders = 
            builders.[numMaintainers..]
        //printfn "numMaintainers: %A" numMaintainers
        //printfn "numBuildings : %A" (List.length currentWorld.Buildings)
        //printfn "agents building new: %A" (List.map (fun el -> el.ID, el.TodaysActivity) newBuilders)
        //printfn "buildings: %A" currentWorld.Buildings

        currentWorld.Buildings
        |> List.map (fun el -> shelterAfterDecay el (List.length currentWorld.Buildings) rg numMaintainers em es) // Existing buildings decay
        |> List.filter (fun el -> el >= 0.1) // If shelter health at 0 then its gone
        |> List.append (List.init (newSheltersBuilt newBuilders) (fun _ -> 1.0)) // All new shelters have 100 health

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
            |> List.sortBy (fun el -> el.TodaysActivity |> snd) 
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
    | None -> {agent with Energy = agent.Energy - rb; EnergyDeprecation = rb} // rb is base energy lost
    | Some(quality) -> 
        let energyDepletion = (1.0 - quality * ep) * rb
        {agent with Energy = agent.Energy - energyDepletion; EnergyDeprecation = energyDepletion} // ep is max shelter energy preservation
