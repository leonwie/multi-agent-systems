module Agent

open Types
open Voting

// Initialise Agent
let initialiseAgent (id : int) (numAgents : int) : Agent =
    let agentParams : float list = // Can change this function to a more complex thing
        List.init 5 (fun el -> (System.Random().Next(0, 100)) |> float)
    {
        Name = "Agent " + (id |> string);
        Selflessness = agentParams.[0] / 100.0; 
        BuildingAptitude = agentParams.[1]; 
        HuntingAptitude = agentParams.[2]; 
        PoliticalApathy = agentParams.[3]; 
        FavouriteFood = // For when it comes to hunting staggi or robbos
            match agentParams.[4] with
                | x when x < 50.0 -> Staggi
                | _ -> Rabbos
        Mood = 100;
        Energy = 100.0;
        TodaysActivity = Nothing, 0.0;
        Opinions = List.init numAgents (fun el -> "Agent " + (el |> string), 50.0) // Default opinions are 50 and can increase or decrease
    }

// Returns whether the agent will build or hunt based on their skills and how much energy they have left
let buildOrHunt (agent : Agent) : Agent =
    let activity =
        agent.BuildingAptitude
        |> (-) agent.HuntingAptitude
        |> function
            | x when x < 0.0 && abs agent.Energy > 20.0 -> Building
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

    // Changes some agents so that that there is an eben number of agents hunting and building
    List.fold (
        fun acc el -> 
            if (el.TodaysActivity |> fst = (toChange |> fst)) && (acc |> snd) > 0
            then ((fst acc) @ [switchAgentActivity el], (snd acc) - 1)
            else ((fst acc) @ [el], (snd acc))
        ) ([], toChange |> snd) allAgents
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

// Vote on what to hunt
let voteOnWhatToHunt (votingSystem : VotingSystem) (agents : Agent list) : Fauna =
    let getFaunaRanking (agent : Agent) : Fauna list =
        match agent.FavouriteFood with
        | Staggi -> [Staggi; Rabbos] // If more animals added then set FavouriteFood as Head and randomise Tail order
        | Rabbos -> [Rabbos; Staggi]       
    agents
    |> List.map (fun el -> getFaunaRanking el)
    |> match votingSystem with
        | Borda -> bordaVote
        | Approval -> approvalVote
        | InstantRunoff -> instantRunoffVote [Staggi; Rabbos]
        | Plurality -> pluralityVote 
            

