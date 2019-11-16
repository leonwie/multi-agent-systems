// Learn more about F# at http://fsharp.org

open System
open Agent
open Types
open Voting

[<EntryPoint>]
let main argv =
    
    // Testing shit
    let numAgents = 4
    
    
    let agents = List.init numAgents (fun el -> initialiseAgent el numAgents)
    let whatToDo (agents : Agent list) =
        agents
        |> List.map buildOrHunt
        |> makeFair
        |> List.map howMuchEnergyToExpend
        |> List.map (fun el ->
            "\n" + el.Name +
            " will do " + (el.TodaysActivity |> fst |> string) +
            " and expend " + (el.TodaysActivity |> snd |> string) + " energy."
        )
    
    printfn "%A" agents
    printfn "\n%A" (whatToDo agents)

    let a = ["a"; "b"; "c";]
    let b = ["b"; "a"; "c";]
    let c = ["c"; "b"; "a";]
    
    printf "\nborda winner %A" (bordaCount [a; b; c])



    0 // return an integer exit code

