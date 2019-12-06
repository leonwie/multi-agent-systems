module multi_agent_systems.Parsing

open Argu
open FSharp.Data.Runtime.BaseTypes
open FSharp.Data
open System.IO
open Agent1

type CLIArguments =
    | Number_Days of days:int
    | Number_Agents of agents:int
    | Number_Profiles of profiles:int
    | Defaults
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Number_Days _ -> "specify the number of days the simulation should run for."
            | Number_Agents _ -> "specify the number of agents you want in your simulation."
            | Number_Profiles _ -> "specify the number of different profiles for agents."
            | Defaults _ -> "only default agents."

let parseActivity = function
   | "NONE" -> Activity.NONE
   | "HUNTING" -> Activity.HUNTING 
   | "BUILDING" -> Activity.BUILDING
   | activity -> failwith (sprintf "Wrong activity type! Only supported: NONE, HUNTING, BUILDING. Activity your agent has : %s" activity)

let mutable Agents:List<Agent> = []

type AgentRead = JsonProvider<"Agent-Config/default_agent.json">

let parseProfile (fileName : string) =
    //let file = File.ReadAllLines("../../../Agent-Config/default_agent.json") |> String.concat " "
    let file = File.ReadAllLines(fileName) |> String.concat " "
    let agentParsed = AgentRead.Parse(file)
    let initialiseAgentHere number = initialiseAgent (agentParsed.Profile, number, agentParsed.Selflessness, agentParsed.BuildingAptitude,
                                       agentParsed.HuntingAptitude, agentParsed.Political, agentParsed.Mood, agentParsed.Energy,
                                       parseActivity agentParsed.Activity, agentParsed.AccessToShelter, agentParsed.Opinions)   
    List.map(fun number -> Agents<-initialiseAgentHere number::Agents) (Array.toList agentParsed.IdRange)
 
let parseAgents (numberProfiles : int) =
    let path : string = "../../../Agent-Config/agent_dir/profile"
    let jsonSuffix : string = ".json" 
    let fileNames =  [0..numberProfiles-1] |> List.map string |> List.map (fun key -> path + key + jsonSuffix)
    List.map(fun file -> parseProfile file) fileNames

let parse (argv : string[])  =
    let parser = ArgumentParser.Create<CLIArguments>(programName = "simulation.exe")
    let inputs = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
    match inputs.Contains Number_Agents with
    | true -> parseAgents (inputs.GetResult Number_Agents)
    | false -> (failwith "Must specify number of agents. Please set --number-agents!")

let getParsedAgents : List<Agent> =
    Agents
    
let printAgent int =
   printfn "%A" Agents