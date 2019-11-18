// Learn more about F# at http://fsharp.org

open System
open Agent
open Types
open Voting
open Activities
open Hunt
open Build
open Config

[<EntryPoint>]
let main argv =
    
    // Testing shit    
    let whatToDo (agents : Agent list) =
        agents
        |> List.map (fun el ->
            "\n" + el.Name +
            " will do " + (el.TodaysActivity |> fst |> string) +
            " and expend " + (el.TodaysActivity |> snd |> string) + " energy."
        )
    
    printfn "%A" agents
    printfn "%A" (whatToDo allAgents)

    printfn "borda winner %A" (voteOnWhatToHunt Borda hunters)
    printfn "plurality winner %A" (voteOnWhatToHunt Plurality hunters)
    printfn "approval winner %A" (voteOnWhatToHunt Approval hunters)
    printfn "instant runnoff winner %A" (voteOnWhatToHunt InstantRunoff hunters)

    let l = List.init 6 id
    let shuffle l =
        let rand = new System.Random()
        let a = l |> Array.ofList
        let swap (a: _[]) x y =
            let tmp = a.[x]
            a.[x] <- a.[y]
            a.[y] <- tmp
        Array.iteri (fun i _ -> swap a i (rand.Next(i, Array.length a))) a
        a |> Array.toList
    printfn "list: %A\nshuffle list: %A" (l) (l |> shuffle)

    0 // return an integer exit code

