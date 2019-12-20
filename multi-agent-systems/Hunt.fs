module Hunt

// Chance of failing a hunt
let failHunt (chanceFail : float) : bool =
    let rand = new System.Random()
    let num = (float (rand.Next(0, 100))) / 100.0
    num >= (1.0 - chanceFail)

// Determines how many hares are captured
let capHare (energyRequired: float) (chanceFail: float) (energyAllocated: float) : float =
    let numHare =
        energyAllocated
        |> (/) energyRequired
        |> floor
        |> int

    // Set failed hunts to 0 and find sum
    seq {for _ in 1 .. numHare -> 1}
    |> Seq.map (fun _ -> if failHunt chanceFail then 0.0 else 1.0)
    |> Seq.sum

let agentAction (ego : float) (suscept : float) (idealism : float) (paySocList : float list) (payIndivList : float list) : int * float =
    let equation (paySoc : float) (payIndiv : float) =
        idealism * paySoc + ego * payIndiv
        |> (/)(idealism * suscept)

    let combine xs ys =
        List.zip xs ys

    combine paySocList payIndivList
    |> List.map (fun (x,y) -> equation x y)
    |> Seq.mapi (fun i v -> i, v)
    |> Seq.maxBy snd

agentAction 5.0 3.0 2.0 [2.0;3.0][4.0;3.0]
|> printfn "%A"

// Check if stag hunt meets criteria for success
let meetStagCondition (actProfile : float list) (weakLink : float) (energyToCapture : float) : bool =
    let minEnergy actProfile weakLink = List.min actProfile >= weakLink
    let thresholdEnergy actProfile energyToCapture = List.sum actProfile >= energyToCapture
    minEnergy actProfile weakLink && thresholdEnergy actProfile energyToCapture

// determines how many stags are captured based on input list of energy allocated to hunt
let capStag (weakLink : float) (collectiveThreshold : float) (chanceFail : float)  (energyAllocated : float list) : float =
    let maxNumStag =
        energyAllocated
        |> List.sum
        |> (/) collectiveThreshold
        |> floor
        |> int

    let numStag =
        seq {for _ in 1 .. maxNumStag -> 1}
        |> Seq.map (fun _ -> if failHunt chanceFail then 0.0 else 1.0)
        |> Seq.sum

    if meetStagCondition energyAllocated weakLink collectiveThreshold then numStag else 0.0

let regenRate (rate : float) (totNum: int) (maxCapacity: int) : int =
    totNum
    |> float
    |> (/) (maxCapacity |> float)
    |> fun x -> 1.0 - x
    |> (*) rate
    |> (*) (totNum |> float)
    |> ceil
    |> int