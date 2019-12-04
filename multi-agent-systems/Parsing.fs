module Parsing

open Argu
open FSharp.Data.Runtime.BaseTypes
open FSharp.Data
open System.IO
open Agent1

type CLIArguments =
    | Number_Days of days:int
    | Number_Agents of agents:int
    | Defaults
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Number_Days _ -> "specify the number of days the simulation should run for."
            | Number_Agents _ -> "specify the number of agents you want in your simulation."
            | Defaults _ -> "only default agents."

let parseActivity = function
   | "NONE" -> Activity.NONE
   | "HUNTING" -> Activity.HUNTING 
   | "BUILDING" -> Activity.BUILDING
   | activity -> failwith (sprintf "Wrong activity type! Only supported: NONE, HUNTING, BUILDING. Activity your agent has : %s" activity)

let mutable Agents:List<Agent1> = []

type AgentRead = JsonProvider<"Agent-Config/default_agent.json">

let parseAgents number_agents : string =     
    let file = File.ReadAllLines("../../../Agent-Config/default_agent.json") |> String.concat " "
    let agentParsed = AgentRead.Parse(file)
    
    for number in agentParsed.IdRange do
       let agent = initialiseAgent1(agentParsed.Profile, number, agentParsed.Selflessness, agentParsed.BuildingAptitude,
                                   agentParsed.HuntingAptitude, agentParsed.Political, agentParsed.Mood, agentParsed.Energy,
                                   parseActivity agentParsed.Activity, agentParsed.AccessToShelter, agentParsed.Opinions)
       Agents <- agent::Agents
    "Parsed"
let printAgent int =
   printfn "%A" Agents
   0

let parse (argv) : string  =
    let parser = ArgumentParser.Create<CLIArguments>(programName = "simulation.exe")
    let inputs = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
    match inputs.Contains Number_Agents with
    | true -> parseAgents (inputs.GetResult Number_Agents)
    | false -> "Must specify number of agents. Please set --number-agents!" 
