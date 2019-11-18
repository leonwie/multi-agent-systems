module Activities

open Types
open Config
open Agent

// Returns whether the agent will build or hunt based on their skills and how much energy they have left
let buildOrHunt (agent : Agent) : Agent =
    let activity =
        agent.BuildingAptitude
        |> (-) agent.HuntingAptitude
        |> function
            | x when x <= 0.0 && abs agent.Energy > 20.0 -> Building
            | x when x > 0.0 && abs agent.Energy > 20.0 -> Hunting
            | _ -> Nothing
    {agent with TodaysActivity = activity, 0.0}


// Splits up group if not enough builders or hunters
// If not enough of one activity then move people to make it fairer
// Mood goes down because they didn't get to what they wanted
let makeFair (allAgents : Agent list) : Agent list =
    // Function for switching an agents activity
    let switchAgentActivity (agent : Agent) : Agent =
        match agent.TodaysActivity with // Half mood if swapped
        | Building, x -> {agent with TodaysActivity = Hunting, x; Mood = agent.Mood / 2}
        | Hunting, x -> {agent with TodaysActivity = Building, x; Mood = agent.Mood / 2}
        | Nothing, _ -> agent

    // Functions for counting the number of each activity
    let activityCount (acc : int * int * int) (x : Agent) : int * int * int =
        let a, b, c = acc |> (fun (a, b, c) -> a, b, c)
        match x.TodaysActivity with
        | Building, _ -> (a + 1, b, c)
        | Hunting, _ -> (a, b + 1, c)
        | Nothing, _ -> (a, b, c + 1)

    let totals =
        List.fold (fun acc x -> activityCount acc x) (0, 0, 0) allAgents

    // Returns a tuple explaing which activity needs changing and how many need changing
    let toChange = 
        totals 
        |> fun (a, _, _) -> a 
        |> (-) (totals |> fun (_, b, _) -> b)
        |> function
            | x when x < -1 -> (Building, abs x / 2)
            | x when x > 1 -> (Hunting, abs x / 2)
            | _ -> (Nothing, 0)

    // Shuffle so that different agents get swapped each day
    let shuffle l =
        let rand = new System.Random()
        let a = l |> Array.ofList
        let swap (a: _[]) x y =
            let tmp = a.[x]
            a.[x] <- a.[y]
            a.[y] <- tmp
        Array.iteri (fun i _ -> swap a i (rand.Next(i, Array.length a))) a
        a |> Array.toList
    
    // Changes some agents so that that there is an eben number of agents hunting and building
    allAgents
    |> shuffle
    |> List.fold (
        fun acc el -> 
            if (el.TodaysActivity |> fst = (toChange |> fst)) && (acc |> snd) > 0
            then ((fst acc) @ [switchAgentActivity el], (snd acc) - 1)
            else ((fst acc) @ [el], (snd acc))
        ) ([], toChange |> snd) 
    |> fst


// Return how much of there energy (as an absolute value) to dedicate to the task based on selflessness and energy
let howMuchEnergyToExpend (agent : Agent) : Agent =
    let energyExpend =
        match agent.TodaysActivity with
            | Building, _ -> agent.BuildingAptitude
            | Hunting, _ -> agent.HuntingAptitude
            | Nothing, _ -> 0.0
        |> (*) agent.Selflessness
        |> (*) (agent.Energy / 100.0)
    {agent with TodaysActivity = agent.TodaysActivity |> fst, energyExpend}


// Decide what to do
let whatToDo (agents : Agent list) =
    agents
    |> List.map buildOrHunt
    |> makeFair
    |> List.map howMuchEnergyToExpend


let allAgents =
    agents 
    |> whatToDo


let builders =
    allAgents
    |> List.filter (fun el -> fst el.TodaysActivity = Building)


let hunters =
    allAgents
    |> List.filter (fun el -> fst el.TodaysActivity = Hunting)