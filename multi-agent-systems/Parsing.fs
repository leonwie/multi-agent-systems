module Parsing

open Argu
open FSharp.Data
open System.IO
open Agent
open Types
open Config

type CLIArguments =
    | Number_Days of days:int
    | Number_Profiles of profiles:int
    | Number_Agents of agents:int
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Number_Days _ -> "specify the number of days the simulation should run for or -1 if infinitely."
            | Number_Profiles _ -> "specify the number of different profiles for agents."
            | Number_Agents _ -> "specify the number of agents."

let parseActivity = function
   | "NONE" -> Types.NONE
   | "STAG" -> Types.STAG
   | "HARE" -> Types.HARE
   | "BUILDING" -> Types.BUILDING
   | activity -> failwith (sprintf "Wrong activity type! Only supported: NONE, HUNTING, BUILDING. Activity your agent has : %s" activity)

type AgentRead = JsonProvider<"Agent-Config/default_agent.json">

let parseOpinions (opinions : decimal[]) : (int * float) list =
    [0..opinions.Length - 1] |> List.map (fun key -> (key, (float)opinions.[key]))

let private parseProfile (fileName : string) : Agent list =
    //let file = File.ReadAllLines("../../../Agent-Config/default_agent.json") |> String.concat " "
    let file = File.ReadAllLines(fileName) |> String.concat " "
    let agentParsed = AgentRead.Parse(file)
    let start = agentParsed.IdRange.[0]
    List.map(fun number -> initialiseAgent number ((float)agentParsed.Susceptibility.[number - start]) ((float)agentParsed.Egotism.[number - start]) ((float)agentParsed.Idealism.[number - start])) (Array.toList agentParsed.IdRange)
 
let private parseAgents (numberProfiles : int) : Agent list =
    let path : string = "../../../Agent-Config/agent_dir/profile"
    let jsonSuffix : string = ".json" 
    let fileNames =  [0..numberProfiles-1] |> List.map string |> List.map (fun key -> path + key + jsonSuffix)
    initialiseAgentDecisions (List.concat (List.map(fun file -> parseProfile file) fileNames))


let parse (argv : string[]) : Agent list =
    let parser = ArgumentParser.Create<CLIArguments>(programName = "simulation.exe")
    let inputs = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
    maxSimulationTurn <- if inputs.Contains Number_Days then inputs.GetResult Number_Days else -1
    numAgents <- if inputs.Contains Number_Agents then inputs.GetResult Number_Agents
                                                  else failwith "Specify a number of agents!"
    match inputs.Contains Number_Profiles with
    | true -> parseAgents (inputs.GetResult Number_Profiles)
    | false -> (failwith "Must specify number of profiles. Please set --number-profiles!")