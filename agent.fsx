type Activity =
    | Building
    | Hunting
    | Nothing

type Fauna = 
    | Rabbos
    | Staggi

type Agent = {
    Name : string;
    Selflessness : float; 
    BuildingAptitude : float;
    HuntingAptitude : float;
    PoliticalApathy : float;
    FavouriteFood : Fauna;
    Mood : int;
    Energy : float;
    TodaysActivity : Activity * float;
    Opinions : (string * float) list
    }

let numAgents = 4


// Initialise Agent
let initialiseAgent (id : int) : Agent =
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
    // Functions for three-tuples
    let frst = fun (a, _, _) -> a
    let scnd = fun (_, b, _) -> b
    let thrd = fun (_, _, c) -> c
    // Function for switching an agents activity
    let switchAgentActivity (agent : Agent) : Agent =
        match agent.TodaysActivity with // Half mood if swapped
        | Building, x -> {agent with TodaysActivity = Hunting, x; Mood = agent.Mood / 2}
        | Hunting, x -> {agent with TodaysActivity = Building, x; Mood = agent.Mood / 2}
        | Nothing, x -> agent
    // Functions for counting the number of each activity
    let activityCount (acc : int * int * int) (x : Agent) : int * int * int =
        match x.TodaysActivity with
        | Building, _ -> ((acc |> frst) + 1, (acc |> scnd), (acc |> thrd))
        | Hunting, _ -> ((acc |> frst), (acc |> scnd) + 1, (acc |> thrd))
        | Nothing, _ -> ((acc |> frst), (acc |> scnd), (acc |> thrd) + 1)
    let totals =
        List.fold (fun acc x -> activityCount acc x) (0, 0, 0) allAgents
    // Returns a tuple explaing which activity needs changing and how many need changing
    let toChange = 
        totals 
        |> frst 
        |> (-) (totals |> scnd)
        |> function
            | x when x < -1 -> (Building, abs x / 2)
            | x when x > 1 -> (Hunting, abs x / 2)
            | _ -> (Nothing, 0)
    printf "%A" (totals, toChange)
    let mutable numToChange = toChange |> snd
    // Changes some agents so that that there is an eben number of agents hunting and building
    List.map (fun el -> 
        if (el.TodaysActivity |> fst = (toChange |> fst)) && numToChange > 0
        then 
            numToChange <- numToChange - 1
            switchAgentActivity el
        else el
        ) allAgents

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


// Testing shit

let agents = List.init numAgents (fun el -> initialiseAgent el)
let whatToDo (agents : Agent list) =
    agents
    |> List.map buildOrHunt
    |> makeFair
    |> List.map howMuchEnergyToExpend
    |> List.map (fun el ->
        "\n" + el.Name +
        " wants to do " + (el.TodaysActivity |> fst |> string) +
        " expending " + (el.TodaysActivity |> snd |> string) + " energy."
    )

printfn "%A" agents
printfn "\n%A" (whatToDo agents)


