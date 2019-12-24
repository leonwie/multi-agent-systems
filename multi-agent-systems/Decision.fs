module multi_agent_systems.Decision
open Types

//#light
open System
open Types

let rand = System.Random()
// Generate count random numbers in [0,1]
let generateRandom length =
    let seed = System.Random()
    List.init length (fun _ -> seed.NextDouble())
let standardize (distributions : float list) = //: float list * float * float =
    let mean = List.average distributions
    let len = List.length distributions |> float
    let standardDeviation = (/) (List.sumBy (fun x -> (x - mean)*(x - mean)) distributions) len |> sqrt
    List.map (fun x -> (x - mean)/(4.0*standardDeviation)+0.5) distributions
    |> List.map (fun x -> match x with
                                | s when s < 0.0 -> 0.0
                                | s when s > 1.0 -> 1.0
                                | s -> s )  //, mean, standardDeviation, len, standardDeviation
//standardize [0.1; 0.2; 0.3; 0.4; 0.9; 0.95; 0.99; 0.88; 0.97]                        
let worldProp : WorldProperties = {
        // just some random values I used for testing
        Tau = 10.0
        Gamma = 5.0
}

let RLalg (choices : float list) (world : WorldState) = //currentDay gamma tau = //(world : WorldState) =
    //let epsilon = exp (- (float world.CurrentDay)/world.Gamma)
    let epsilon = exp (- (float world.CurrentDay) / worldProp.Gamma)
    //let input = 
    let maxim = List.max choices
    let indexedChoices = choices |> List.mapi (fun id x -> (id,x))
    let bestOptions = indexedChoices |> List.filter (fun x -> (snd x) = maxim)
    // if there is more than one best option don't eliminate any of them from the exploration options
    // if there is only one best option then remove it from the options for exploration
    let choicesWithoutMax = match bestOptions |> List.length with
                            | 1 -> indexedChoices |> List.filter (fun x -> snd x <> maxim)
                            | _ -> indexedChoices
    printfn "choices : %A" choicesWithoutMax
    printfn "best options %A" bestOptions
    let softmax x = exp (x / worldProp.Tau)
    // choices instead of choicesWithoutMax
    let newChoices = List.map (fun x -> fst x, softmax (snd x)) choicesWithoutMax
    let softmaxMapping = newChoices |> List.map (fun x -> fst x, (snd x) / (List.sumBy snd newChoices))
    let rndNo = generateRandom 1
    printfn "first random no %A" rndNo
    let explore (list:(int*float) list) =
        let rnd = generateRandom 1
        printfn "second random no %A" rnd
        let indexedRanges = list |> List.fold (fun acc x -> List.append (fst acc) [(snd acc),(snd acc) + (snd x), fst x], (snd acc) + (snd x)) (List.empty,0.0)
        let predicate (x:(float*float*int)) = match x with
                                              | low, high, index when low <= rnd.Head && high > rnd.Head -> true
                                              //| sol when sol >= fst (snd x) && sol <  snd (snd x) -> true
                                              | _ -> false
        printfn "ranges: %A" indexedRanges
        match fst indexedRanges |> List.filter predicate |> List.tryHead with
        | Some (_,_,id) -> id
        | _ -> failwith("random number out of range")
   // printfn "exploration result %A" (explore softmaxMapping)
    printfn "epsilon = %A" epsilon
    match rndNo with
    | head::_ when head < epsilon -> fst bestOptions.Head
    | _ -> explore softmaxMapping
//RLalg [0.1; 0.15; 0.2 0.15; 0.1; 0.1] 10.0 
let workAllocation (agent:Agent) (world:WorldState) =
   // let reward = float agent.Gain - agent.EnergyConsumed
    let ego = agent.Egotism / (agent.Egotism + agent.Idealism)
    let ideal = agent.Idealism / (agent.Egotism + agent.Idealism)
    let opinion = List.map2 (fun x y -> ego*x + ideal*y) agent.R agent.S
    RLalg opinion world

let foodSharing (agent:Agent) (world:WorldState) =
    match agent.Egotism - agent.Idealism with
    | negative when negative < 0.0 -> 1 // assuming the second entry in the list of payoffs is for sharing 
    | _ -> RLalg agent.Rsharing world // return 1 for sharing and 0 for keeping all food 
 
                   

//let computeSocialGood (agents : Agent list) (world : WorldState) =
    //let socialGood = List.sumBy (fun x -> x.reward) agents - world.energyDepreciation
//    let socialGood2 = List.averageBy (fun x -> x.Reward - x.EnergyDeprecation) agents
//    {world with AverageSocialGood = socialGood2}
   
//let updateRewardAverage (agent : Agent) (world : WorldState) : Agent =
//    let avgReward = match float world.CurrentDay with
//                        | 0.0 -> agent.Reward
//                        | t -> (agent.AverageReward*t + agent.Reward)/(t+1.0)
//    {agent with AverageReward = avgReward}

//let computeSocialReward (world : WorldState) : WorldState =
//    let avgSocialReward = match float world.CurrentDay with
//                              | 0.0 -> world.GlobalSocialGood
//                              | t -> (world.AverageSocialGood*t + world.GlobalSocialGood)/(t+1.0)
//    {world with AverageSocialGood = avgSocialReward}


// Vectors and matrices are in the Extreme.Mathematics
// namespace
//open Extreme.Mathematics
// The linear programming classes reside in their own namespace.
//open System
//open System.Collections.Generic
//open Extreme.Mathematics.Optimization
/// maximum level of energy
//let E = 100.0
/// a large enough value to act as infinity in our LP
//let inf = 1000.0
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
//let bodyForRules = Matrix.Create(4, 4, [|
//            1.0;   1.0;   0.0;  0.0;     // 1.0*Xbuild + 1.0*Xhunt <= e
//            1.0;   0.0;    -E;  0.0;     // 1.0*Xbuild - E*alpha <= 0
//            0.0;   1.0;     E;  0.0;     // 1.0*Xhunt + E*alpha <= E
//           -1.0;  -1.0;   0.0;   -E;     // -1.0*Xbuild -1.0*Xbuild - E*Xsanction1 <= -M || minimum contribution of M energy
//        |], MatrixElementOrder.RowMajor)

//let decide (constraints:LinearAlgebra.DenseMatrix<float>) currentEnergy contVars discVars (rules:(string*float) []) =
/// instantiate an object of type LinearProgram with no objective function, no constraints and no decision variables 
//    let lp = LinearProgram()
/// iterate through the list of continuous decision variables and add them one by one to the LP, the cost/payoff is added to the objective function as well
//    List.iter (fun (name, payoff) -> lp.AddVariable(name, payoff, 0.0, currentEnergy) |> ignore) contVars
/// same thing as above but for binary variables this time
//    List.iter (fun (name, payoff) -> lp.AddBinaryVariable(name, payoff) |> ignore) discVars 
/// take the array of rules, where each element consists of a string describing the rule and a float which has the value associated with the rule, and get rid of the strings
//    let ruleValues = Array.map snd rules
/// construct a list where each element is a tuple containing: 1) the id and 2) the corresponding row in the "constraints matrix"
//    let rows = List.map (fun (id:int) -> id, constraints.GetRow id) [0..constraints.Rows.Count-1]
/// iterate through the list of rows and add a constraint to the LP for each one them 
//    List.iter (fun (id,constr) -> (lp.AddLinearConstraint("C" + string id, constr, -inf, ruleValues.[id])|>ignore)) rows 
/// solve the LP and store the solution vector in a variable
//    let solutions = lp.Solve()
/// convert the list of rule values to a vector in order to perform vector arithmetic
//    let b = Vector.Create(ruleValues)
/// calculate the slack values for each one of the constraints;
/// values of 0.0 mean that the constraint was barely satisfied by the optimal solution found;
/// this is a good indication that the agent is unhappy with the rule associated with that specific constraint as it might be limiting his expected reward
//    let slacks = constraints*solutions - b
/// sort the array of slacks and return the indexes of the constraints that have a slack value of 0.0
//    let pen = slacks.ToArray() |> Array.mapi (fun id slack -> (id, slack))
//    let penal = pen |> Array.filter (fun (_, slack) -> slack = 0.0) |> Array.map fst
//    printfn "slacks: %A" pen
//    printfn "solution: %A" (solutions.ToString("F"))
/// return:
/// 1) the ids of the constraints the agent is unhappy about
/// 2) the vector of solutions
/// 3) the value of the expected reward
//    (penal, solutions, lp.OptimalValue)
    
/// external input
//    let e = 30.0
//    let M = 10.0
//    let laws = [|("agents cannot spend more energy than what they have", e);
//                 ("1st half of: agents can either hunt or build", 0.0);
//                 ("2nd half of: agents can either hunt or build", E);
//                 ("agents that can work must spend at least M energy", -M)|]
//    let varFloat = [("Xbuild", -7.0); ("Xhunt", -3.0)] 
//    let varBool = [("alpha", 0.0); ("Xsanction1", 9.0)]
//    printfn "Solution: %A" (decide bodyForRules e varFloat varBool laws)


