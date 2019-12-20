module Parsing

open Argu
open FSharp.Data
open System.IO
open Agent
open Types

type CLIArguments =
    | Number_Days of days:int
    | Number_Profiles of profiles:int
    | Defaults
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Number_Days _ -> "specify the number of days the simulation should run for."
            | Number_Profiles _ -> "specify the number of different profiles for agents."
            | Defaults _ -> "only default agents."

let parseActivity = function
   | "NONE" -> Types.NONE
   | "HUNTING" -> Types.HUNTING 
   | "BUILDING" -> Types.BUILDING
   | activity -> failwith (sprintf "Wrong activity type! Only supported: NONE, HUNTING, BUILDING. Activity your agent has : %s" activity)

type AgentRead = JsonProvider<"Agent-Config/default_agent.json">

let parseOpinions (opinions : decimal[]) : (int * float) list =
    [0..opinions.Length - 1] |> List.map (fun key -> (key, (float)opinions.[key]))

let parseProfile (fileName : string) : Agent list =
    //let file = File.ReadAllLines("../../../Agent-Config/default_agent.json") |> String.concat " "
    let file = File.ReadAllLines(fileName) |> String.concat " "
    let agentParsed = AgentRead.Parse(file)
    let initialiseAgentHere number = initialiseAgent agentParsed.Profile number ((float)agentParsed.Selflessness)
                                       ((float)agentParsed.BuildingAptitude) ((float)agentParsed.HuntingAptitude)
                                       ((float)agentParsed.Political) agentParsed.Mood ((float)agentParsed.Energy)
                                       (parseActivity agentParsed.ActivityType, (float)agentParsed.ActivityNumber)
                                       (Some ((float)agentParsed.AccessToShelter)) (parseOpinions agentParsed.Opinions)   
    
    List.map(fun number -> initialiseAgentHere number) (Array.toList agentParsed.IdRange)
 
let parseAgents (numberProfiles : int) : Agent list=
    let path : string = "../../../Agent-Config/agent_dir/profile"
    let jsonSuffix : string = ".json" 
    let fileNames =  [0..numberProfiles-1] |> List.map string |> List.map (fun key -> path + key + jsonSuffix)
    List.concat (List.map(fun file -> parseProfile file) fileNames)


let parse (argv : string[]) : Agent list =
    let parser = ArgumentParser.Create<CLIArguments>(programName = "simulation.exe")
    let inputs = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
    match inputs.Contains Number_Profiles with
    | true -> parseAgents (inputs.GetResult Number_Profiles)
    | false -> (failwith "Must specify number of agents. Please set --number-profiles!")