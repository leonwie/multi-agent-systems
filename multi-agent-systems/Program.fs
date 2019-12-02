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

    let allAgents =
        agents 
        |> jobAllocation


    let builders =
        allAgents
        |> List.filter (fun el -> fst el.TodaysActivity = Building)


    let hunters =
        allAgents
        |> List.filter (fun el -> fst el.TodaysActivity = Hunting)
    
    printfn "%A" agents
    printfn "%A" (whatToDo allAgents)

    printfn "borda winner %A" (voteOnWhatToHunt Borda hunters)
    printfn "plurality winner %A" (voteOnWhatToHunt Plurality hunters)
    printfn "approval winner %A" (voteOnWhatToHunt Approval hunters)
    printfn "instant runnoff winner %A" (voteOnWhatToHunt InstantRunoff hunters)

    let hunting = voteOnWhatToHunt Borda hunters

    let test = 
        hunters
        |> hunt hunting huntingTime


    printfn "Leftovers: %A" test

    Console.ReadLine() // Wait for user input before closing

    0 // return an integer exit code

