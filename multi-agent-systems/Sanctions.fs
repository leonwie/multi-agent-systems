module Sanctions

open Types
open Config

// Breakable Rules
// 1) Agents not sharing food
// 2) Agents not doing as told

// Calculate the ideal allocation according to rule
// This means returning expected energy gain and a job allocation
// Since sanction does not check for job type as long as there is a job
// Actual job allocation does not matter


// CAUTION: NO SPECS ABOUT AGENT DECISION ON JOB OR ENERGY TO SHARE
let idealAllocation (world: WorldState) (agents: Agent list) (totalFoodShared: float): float list * Activity list = 

    let totalEnergy = 
        agents
        |> List.map (fun el -> el.Energy)
        |> List.sum

    let totalEffort =
        agents
        |> List.map (fun el -> snd el.TodaysActivity)
        |> List.sum
    
    let targetEnergyList = 
        agents
        |> List.map (fun el ->
            match world.CurrentFoodRule with
            | Communism -> totalFoodShared / (agents |> List.length |> float)
            | FoodRule.Socialism -> totalFoodShared * (1.0 - el.Energy / totalEnergy)
            | FoodRule.Meritocracy -> totalFoodShared * (snd el.TodaysActivity) / totalEffort
            | FoodRule.Oligarchy -> el.Energy / totalEnergy * (totalFoodShared - MinimumFoodForOligarchy) + MinimumFoodForOligarchy
        )

    let targetWorkStatus = 
        agents
        |> List.map (fun el ->
            match world.CurrentWorkRule with
            | Everyone -> 
                HUNTING // Indicate that agent will have activity
            | Strongest -> 
                if el.Energy >= WorkExemptionThreshold then HUNTING else NONE
            | _ -> NONE // No expectation on working
        )

    (targetEnergyList, targetWorkStatus)

// Allocate food according to precomputed assignment
let allocateFood (targetEnergyList: float list) (agents: Agent list): Agent list = 
    List.zip agents targetEnergyList
    |> List.map (fun (agent, energy) ->
        if agent.AccessToFood = true
        then {agent with Energy = agent.Energy + energy;
                         TodaysEnergyObtained = agent.TodaysEnergyObtained + energy}    
        else agent
    )


// Update at end-of-day
let infamyDecay (world: WorldState) (agents: Agent list) : Agent list =
    agents
    // Halve infamy every 8 days
    |> List.map (fun el ->
        if world.CurrentDay <> el.LastCrimeDate then
            match (world.CurrentDay - el.LastCrimeDate) % 8 with
            | 0 -> {el with Infamy = el.Infamy / 2.0 |> floor}
            | _ -> el
        else el
    )

// Sanction (change accessibility state) based on rules
let sanction (world: WorldState) (agents: Agent list) : Agent list = 
    agents
    |> List.map (fun el ->
        if el.Infamy <= 0.2 then {el with AccessToShelter = Some 1.0;
                                            AccessToFood = true}
        elif el.Infamy <= 0.5 then {el with AccessToShelter = None;
                                            AccessToFood = true}
        elif el.Infamy <= 0.9 then {el with AccessToFood = false;
                                            AccessToShelter = None}
        else 
            match world.CurrentMaxPunishment with 
            | NoFoodAndShelter -> {el with AccessToFood = false;
                                            AccessToShelter = None}
            | Exile -> {el with Alive = false}
            | _ -> failwith "Invalid maximum punishment setting"
    )


// Detect crime actions based on probability of discovery
let detectCrime (world: WorldState) (expectedEnergyGain: float list) (expectedWorkType: Activity list) (agents: Agent list) : Agent list =
   
    let rand = new System.Random()
    let checkFoodAllocation (agents: Agent list) = 
        List.zip agents expectedEnergyGain
        |> List.map (fun (agent, gain) ->
            if agent.TodaysEnergyObtained > gain && rand.NextDouble() < CrimeDiscoveryRate 
                then {agent with Infamy = min 1.0 agent.Infamy + InfamyStep; LastCrimeDate = world.CurrentDay}
            else agent
        )

    let checkTaskExecution (agents: Agent list) = 
        List.zip agents expectedWorkType
        |> List.map (fun (agent, job) ->
            if fst agent.TodaysActivity = NONE && job <> NONE && rand.NextDouble() < CrimeDiscoveryRate 
                then {agent with Infamy = min 1.0 agent.Infamy + InfamyStep; LastCrimeDate = world.CurrentDay}
            else agent
        )

    agents
    |> checkFoodAllocation
    |> checkTaskExecution

