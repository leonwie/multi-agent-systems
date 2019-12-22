module multi_agent_systems.Opinion
open Config
open Types

let rec private lookForAgentByID (agentList : Agent list) (id : int) : Agent option =
    match agentList with
    | [] -> None
    | a :: _ when a.ID = id -> Some a
    | _ :: tail -> lookForAgentByID tail id

// Computes median of a sequence    
let private median list = 
    let sorted = list |> Seq.toArray |> Array.sort
    let firstHalf, secondHalf = 
        let length = sorted.Length - 1 |> float
        length / 2. |> floor |> int, length / 2. |> ceil |> int 
    (sorted.[firstHalf] + sorted.[secondHalf] |> float) / 2.

// Calculated g_i aka g_agent where i = agent.ID
let g (agent : Agent) : float = max (1.0 - 1.2 * agent.Susceptibility) 0.0

let private updateRuleOpinionPerAgent (agents : Agent list) (agent : Agent) : Agent =
    // g * X(0)
    let initialRuleOpinion = g agent * agent.DecisionOpinions.Value.InitialRuleOpinion.[agent.ID]
    let ruleOpinion = agent.DecisionOpinions.Value.CurrentRulesOpinion
    let otherOpinion index = List.map snd (lookForAgentByID agents index).Value.DecisionOpinions.Value.OtherAgentsOpinion    
    // (1 - g) * a[row] * X(t)
    let oneRuleChange row = initialRuleOpinion + List.sum (List.map (fun index ->
        (1.0 - g agent) * ruleOpinion.[index] * (otherOpinion row).[index]) [0..numberOfRules - 1])
    let updatedRuleOpinion = List.map (fun row -> oneRuleChange row) [0..numAgents - 1]
    let updatedDecision = {agent.DecisionOpinions.Value with CurrentRulesOpinion = updatedRuleOpinion}
    {agent with DecisionOpinions = Some updatedDecision}

let private updateReward (agent : Agent) : Agent =
    let newReward = (float)agent.Gain - agent.EnergyConsumed
    {agent with Reward = newReward}

// Returns agents with modified agent RuleOpinion = G * X(0) + (I - G) * A * X(t)        
let updateRuleOpinion (agents : Agent list) : Agent list =
    let applyFunction agent = updateRuleOpinionPerAgent agents agent
    List.map applyFunction agents

// reward_i=gain_i-energyConsumed_i    
let calculateUpdatedRewards (agents : Agent list) : Agent list =
    List.map updateReward agents

// socialGood= MedianOf(reward_i-energyDepreciation_i)    
let calculateGlobalSocialGood (agents : Agent list) (state : WorldState) : WorldState =
    let rewEnergyDepDiff = List.map (fun agent -> agent.Reward - agent.EnergyDeprecation) agents
    {state with GlobalSocialGood = median rewEnergyDepDiff}