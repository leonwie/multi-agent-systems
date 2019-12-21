module multi_agent_systems.Decision

#light
open System
// Vectors and matrices are in the Extreme.Mathematics
// namespace
open Extreme.Mathematics
// The linear programming classes reside in their own namespace.
open System
open System.Collections.Generic
open Extreme.Mathematics.Optimization
/// maximum level of energy
let E = 100.0
/// a large enough value to act as infinity in our LP
let inf = 1000.0
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
let bodyForRules = Matrix.Create(4, 4, [|
            1.0;   1.0;   0.0;  0.0;     // 1.0*Xbuild + 1.0*Xhunt <= e
            1.0;   0.0;    -E;  0.0;     // 1.0*Xbuild - E*alpha <= 0
            0.0;   1.0;     E;  0.0;     // 1.0*Xhunt + E*alpha <= E
           -1.0;  -1.0;   0.0;   -E;     // -1.0*Xbuild -1.0*Xbuild - E*Xsanction1 <= -M || minimum contribution of M energy
        |], MatrixElementOrder.RowMajor)

let decide (constraints:LinearAlgebra.DenseMatrix<float>) currentEnergy contVars discVars (rules:(string*float) []) =
/// instantiate an object of type LinearProgram with no objective function, no constraints and no decision variables 
    let lp = LinearProgram()
/// iterate through the list of continuous decision variables and add them one by one to the LP, the cost/payoff is added to the objective function as well
    List.iter (fun (name, payoff) -> lp.AddVariable(name, payoff, 0.0, currentEnergy) |> ignore) contVars
/// same thing as above but for binary variables this time
    List.iter (fun (name, payoff) -> lp.AddBinaryVariable(name, payoff) |> ignore) discVars 
/// take the array of rules, where each element consists of a string describing the rule and a float which has the value associated with the rule, and get rid of the strings
    let ruleValues = Array.map snd rules
/// construct a list where each element is a tuple containing: 1) the id and 2) the corresponding row in the "constraints matrix"
    let rows = List.map (fun (id:int) -> id, constraints.GetRow id) [0..constraints.Rows.Count-1]
/// iterate through the list of rows and add a constraint to the LP for each one them 
    List.iter (fun (id,constr) -> (lp.AddLinearConstraint("C" + string id, constr, -inf, ruleValues.[id])|>ignore)) rows 
/// solve the LP and store the solution vector in a variable
    let solutions = lp.Solve()
/// convert the list of rule values to a vector in order to perform vector arithmetic
    let b = Vector.Create(ruleValues)
/// calculate the slack values for each one of the constraints;
/// values of 0.0 mean that the constraint was barely satisfied by the optimal solution found;
/// this is a good indication that the agent is unhappy with the rule associated with that specific constraint as it might be limiting his expected reward
    let slacks = constraints*solutions - b
/// sort the array of slacks and return the indexes of the constraints that have a slack value of 0.0
    let pen = slacks.ToArray() |> Array.mapi (fun id slack -> (id, slack))
    let penal = pen |> Array.filter (fun (_, slack) -> slack = 0.0) |> Array.map fst
    printfn "slacks: %A" pen
    printfn "solution: %A" (solutions.ToString("F"))
/// return:
/// 1) the ids of the constraints the agent is unhappy about
/// 2) the vector of solutions
/// 3) the value of the expected reward
    (penal, solutions, lp.OptimalValue)
    
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