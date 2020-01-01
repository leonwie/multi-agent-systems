module Decision
open Config
open Types

let rand = System.Random()
// Generate count random numbers in [0,1]
let generateRandom (length) =
    let seed = System.Random()
    List.init length (fun _ -> seed.NextDouble())
let standardize (distributions : float list) : float list =
    let allEqual = distributions |> Seq.windowed(2) |> Seq.forall(fun arr -> arr.[0] = arr.[1])
    if allEqual then
        List.map (fun _ -> 0.5) distributions
    else    
        let mean = List.average distributions
        let len = List.length distributions |> float
        let standardDeviation = (/) (List.sumBy (fun x -> (x - mean)*(x - mean)) distributions) len |> sqrt
        List.map (fun x -> (x - mean)/(4.0*standardDeviation)+0.5) distributions
        |> List.map (fun x -> match x with
                                    | s when s < 0.0 -> 0.0
                                    | s when s > 1.0 -> 1.0
                                    | s -> s )

let RLalg (choices : float list) (world : WorldState) = //currentDay gamma tau = //(world : WorldState) =

    let epsilon = exp (- (float world.CurrentDay) / Gamma)

    let maxim = List.max choices
    let indexedChoices = choices |> List.mapi (fun id x -> (id,x))
    let bestOptions = indexedChoices |> List.filter (fun x -> (snd x) = maxim)
    // if there is more than one best option don't eliminate any of them from the exploration options
    // if there is only one best option then remove it from the options for exploration
    let choicesWithoutMax = match bestOptions |> List.length with
                            | 1 -> indexedChoices |> List.filter (fun x -> snd x <> maxim)
                            | _ -> indexedChoices
    //printfn "choices : %A" choicesWithoutMax
    //printfn "best options %A" bestOptions
    let softmax x = exp (x / Tau)
    // choices instead of choicesWithoutMax
    let newChoices = List.map (fun x -> fst x, softmax (snd x)) choicesWithoutMax
    let softmaxMapping = newChoices |> List.map (fun x -> fst x, (snd x) / (List.sumBy snd newChoices))
    let rndNo = generateRandom (1)
    //printfn "first random no %A" rndNo
    let explore (list:(int*float) list) =
        let rnd = generateRandom (1)
        //printfn "second random no %A" rnd
        let indexedRanges = list |> List.fold (fun acc x -> List.append (fst acc) [(snd acc),(snd acc) + (snd x), fst x], (snd acc) + (snd x)) (List.empty,0.0)
        let predicate (x:(float*float*int)) = match x with
                                              | low, high, index when low <= rnd.Head && high > rnd.Head -> true
                                              //| sol when sol >= fst (snd x) && sol <  snd (snd x) -> true
                                              | _ -> false
        //printfn "ranges: %A" indexedRanges
        match fst indexedRanges |> List.filter predicate |> List.tryHead with
        | Some (_,_,id) -> id
        | _ -> failwith("random number out of range")
   // printfn "exploration result %A" (explore softmaxMapping)
    //printfn "epsilon = %A" epsilon
    match rndNo with
    | head::_ when head < epsilon -> fst bestOptions.Head
    | _ -> explore softmaxMapping

let workAllocation (agent:Agent) (world:WorldState) =
    let ego = agent.Egotism / (agent.Egotism + agent.Idealism)
    let ideal = agent.Idealism / (agent.Egotism + agent.Idealism)
    let opinion = List.map2 (fun x y -> ego*x + ideal*y) agent.R world.S
    RLalg opinion world // Return decision

let huntStrategyDecision (agent:Agent) (world:WorldState) =
    let ego = agent.Egotism / (agent.Egotism + agent.Idealism)
    let ideal = agent.Idealism / (agent.Egotism + agent.Idealism)
    let opinion = List.map2 (fun x y -> ego*x + ideal*y) agent.RhuntingEnergySplit world.ShuntingEnergySplit
    RLalg opinion world // Return decision

let foodSharing (agent:Agent) (world:WorldState) =
    match agent.Egotism - agent.Idealism with
    | negative when negative < 0.0 -> 1 // assuming the second entry in the list of payoffs is for sharing 
    | _ -> RLalg agent.Rsharing world // return 1 for sharing and 0 for keeping all food

// Helper function to compute cumulative average value for reward and social good updating
let getCumulativeAverage (currentDay: int) (prevAverage: float) (todaysVal: float) = 
    match currentDay with
    | 0 -> todaysVal
    | _ -> 
        prevAverage * (currentDay |> float) + todaysVal
        |> fun x -> x / ((currentDay + 1) |> float)