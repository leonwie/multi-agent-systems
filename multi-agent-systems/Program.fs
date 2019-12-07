
open System
open Agent
open Types
open Voting
open Activities
open Hunt
open Build
open Config
open multi_agent_systems

[<EntryPoint>]
let main argv =
    // Config tested with arguments "--number-days 20 --number-profiles 2"
    let agents = Parsing.parse argv

    let whatToDo (agents : Agent list) =
        agents
        |> List.map (fun el ->
            "\n" + (string)el.ID +
            " will do " + (el.TodaysActivity |> fst |> string) +
            " and expend " + (el.TodaysActivity |> snd |> string) + " energy."
        )

    let allAgents =
        agents 
        |> jobAllocation

    let builders =
        allAgents
        |> List.filter (fun el -> fst el.TodaysActivity = BUILDING)

    let hunters =
        allAgents
        |> List.filter (fun el -> fst el.TodaysActivity = HUNTING)
    
    printfn "%A" agents
    printfn "%A" (whatToDo allAgents)
    
    0 // return an integer exit code
