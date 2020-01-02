module Hunt

open Types
open Config
open Decision

// Chance of failing a hunt
let failHunt (chanceFail : float) : bool =
    let rand = new System.Random()
    let num = (float (rand.Next(0, 100))) / 100.0
    num >= (1.0 - chanceFail)

// Determines how many hares are captured
let capHare (var: Agent list * WorldState) : Agent list * WorldState =
    let agents, world = var
    let mutable currentNumHare = world.NumHare |> float
    let numHareCaptured (agent: Agent) =
        let maxHare =
            agent.TodaysHuntOption
            |> float
            |> (*) 0.1
            |> (*) (float costOfWorking)    // Proportion of energy spent on hare
            |> fun x -> x / rabbosMinRequirement
            |> floor
            |> int

        // Set failed hunts to 0 and find sum
        seq {for _ in 1 .. maxHare -> 1}
        |> Seq.map (fun _ -> if failHunt rabbosProbability then 0.0 else 1.0)
        |> Seq.sum
        |> min currentNumHare   // The captured num must be smaller than global numHare
    
    let hareCapturedPerAgent =
        agents
        |> List.map (fun agent ->
            let numHare = numHareCaptured agent
            currentNumHare <- currentNumHare - numHare
            numHare
        )

    let newAgents = 
        List.zip agents hareCapturedPerAgent
        |> List.map (fun (agent, numHare) -> 
            {agent with HuntedFood = numHare * rabbosEnergyValue}
        )

    let hareDecrease = hareCapturedPerAgent |> List.sum |> int

    (newAgents, {world with NumHare = world.NumHare - hareDecrease})

// Check if stag hunt meets criteria for success
let meetStagCondition (actProfile : float list): bool =

    let minEnergy actProfile = List.min actProfile >= staggiMinIndividual
    let thresholdEnergy actProfile = List.sum actProfile >= staggiMinCollective
    minEnergy actProfile && thresholdEnergy actProfile

// determines how many stags are captured based on input list of energy allocated to hunt
let capStag (var : Agent list * WorldState) : Agent list * WorldState =
    let agents, world = var
    if agents.Length = 0 then (agents, world)
    else
        let actProfile = 
            agents
            |> List.map (fun el -> 
                el.TodaysHuntOption
                |> float
                |> (*) 0.1
                |> fun x -> 1.0 - x
                |> (*) (float costOfWorking)    // Proportion of energy spent on stag
            )

        let maxNumStag =
            actProfile
            |> List.sum
            |> fun x -> x / staggiMinCollective
            |> floor
            |> int

        let numStag =
            seq {for _ in 1 .. maxNumStag -> 1}
            |> Seq.map (fun _ -> if failHunt staggiProbability then 0.0 else 1.0)
            |> Seq.sum
            |> min (world.NumStag |> float)

        let avgStagEnergy = 
            if meetStagCondition actProfile then numStag else 0.0
            |> fun x -> x / (List.length agents |> float)

        let newAgents =
            agents
            |> List.map (fun el -> {el with HuntedFood = avgStagEnergy})

        (newAgents, {world with NumStag = world.NumStag - (numStag |> int)})

let regenRate (rate : float) (totNum: int) (maxCapacity: int) : int =
    totNum
    |> float
    |> fun x -> x / (maxCapacity |> float)
    |> fun x -> 1.0 - x
    |> (*) rate
    |> (*) (totNum |> float)
    |> ceil
    |> int


let shareFood (var: Agent list * WorldState) : Agent list * WorldState=
    let agents, world = var
    let newAgents = 
        agents
        // Share food based on decision-making
        |> List.map (fun el ->
            match foodSharing el world with
            | 0 -> {el with Gain = el.HuntedFood;
                            Energy = el.Energy + el.HuntedFood}
            | _ -> {el with Gain = 0.0;
                            FoodShared = true}
        )
    (newAgents, world)

let hunt (world: WorldState) (hunters: Agent list) : Agent list * WorldState =

    (hunters, world)
    |> capHare
    |> capStag
    |> shareFood
    