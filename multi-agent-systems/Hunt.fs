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
let capHare (agents: Agent list) : Agent list =
    
    let numHareCaptured (agent: Agent) =
        let maxHare =
            snd agent.TodaysActivity
            |> fun x -> x / rabbosMinRequirement
            |> floor
            |> int

        // Set failed hunts to 0 and find sum
        seq {for _ in 1 .. maxHare -> 1}
        |> Seq.map (fun _ -> if failHunt rabbosProbability then 0.0 else 1.0)
        |> Seq.sum
    
    agents
    |> List.map (fun el -> 

        // Calculate food gained by each hunter
        let totalHareEnergy = 
            numHareCaptured el
            |> (*) rabbosEnergyValue

        {el with HuntedFood = totalHareEnergy}
    )

// Check if stag hunt meets criteria for success
let meetStagCondition (actProfile : float list): bool =

    let minEnergy actProfile = List.min actProfile >= staggiMinIndividual
    let thresholdEnergy actProfile = List.sum actProfile >= staggiMinCollective
    minEnergy actProfile && thresholdEnergy actProfile

// determines how many stags are captured based on input list of energy allocated to hunt
let capStag (agents : Agent list) : Agent list =

    if agents.Length = 0 then agents
    else
        let actProfile = 
            agents
            |> List.map (fun el -> snd el.TodaysActivity)

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

        let avgStagEnergy = 
            if meetStagCondition actProfile then numStag else 0.0
            |> fun x -> x / (List.length agents |> float)

        agents
        |> List.map (fun el -> {el with HuntedFood = avgStagEnergy})

let regenRate (rate : float) (totNum: int) (maxCapacity: int) : int =
    totNum
    |> float
    |> fun x -> x / (maxCapacity |> float)
    |> fun x -> 1.0 - x
    |> (*) rate
    |> (*) (totNum |> float)
    |> ceil
    |> int


let shareFood (world: WorldState) (agents: Agent list) : Agent list =

    agents
    // Share food based on decision-making
    |> List.map (fun el ->
        match foodSharing el world with
        | 0 -> {el with Gain = el.HuntedFood}
        | _ -> el
    )
    

// NOTE Final version implemented in Decision.fs

//let agentAction (ego : float) (suscept : float) (idealism : float) (paySocList : float list) (payIndivList : float list) : int * float =
//    let equation (paySoc : float) (payIndiv : float) =
//        idealism * paySoc + ego * payIndiv
//        |> (/)(idealism * suscept)

//    let combine xs ys =
//        List.zip xs ys

//    combine paySocList payIndivList
//    |> List.map (fun (x,y) -> equation x y)
//    |> Seq.mapi (fun i v -> i, v)
//    |> Seq.maxBy snd

//agentAction 5.0 3.0 2.0 [2.0;3.0][4.0;3.0]
//|> printfn "%A"