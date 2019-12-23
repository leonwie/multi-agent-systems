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
    let ruleOpinion = agent.DecisionOpinions.Value.PersonalCurrentRulesOpinion
    let otherOpinion index = List.map snd (lookForAgentByID agents index).Value.DecisionOpinions.Value.AllOtherAgentsOpinion    
    // (1 - g) * a[row] * X(t)
    let oneRuleChange row = initialRuleOpinion + List.sum (List.map (fun index ->
        (1.0 - g agent) * ruleOpinion.[index] * (otherOpinion row).[index]) [0..numberOfRules - 1])
    let updatedRuleOpinion = List.map (fun row -> oneRuleChange row) [0..numAgents - 1]
    let updatedDecision = {agent.DecisionOpinions.Value with PersonalCurrentRulesOpinion = updatedRuleOpinion}
    {agent with DecisionOpinions = Some updatedDecision}

let private updateRewardPerRule (agent : Agent) (newRewardForAgent : float) (rule : Rule) : (Rule * float * LastUpdate) =
    let rewards = agent.DecisionOpinions.Value.RewardPerRule
    let (ruleC, currentReward, lastUpdate) = List.filter (fun (currRule, _, _) -> rule = currRule) rewards |> List.head
    match lastUpdate with
    | 0 -> (ruleC, newRewardForAgent, 1)
    | t -> (ruleC, (currentReward * (float)t + newRewardForAgent) / (float)(t + 1), t + 1)
    
let private updateReward (state : WorldState) (agent : Agent) : Agent =
    let newRewardForAgent = (float)agent.Gain - agent.EnergyConsumed - agent.EnergyDeprecation
    let currentRules = List.map (fun (_, rules, _, _) -> rules) state.CurrentRuleSet
    let updatedRewards = List.map (updateRewardPerRule agent newRewardForAgent) currentRules
    let filteredRewards = List.filter (fun (rule, _, _) -> not(List.contains rule currentRules)) agent.DecisionOpinions.Value.RewardPerRule
    let updatedDecision = {agent.DecisionOpinions.Value with RewardPerRule = filteredRewards @ updatedRewards}
    {agent with DecisionOpinions = Some updatedDecision}

// Returns agents with modified agent RuleOpinion = G * X(0) + (I - G) * A * X(t)        
let updateRuleOpinion (agents : Agent list) : Agent list =
    let applyFunction agent = updateRuleOpinionPerAgent agents agent
    List.map applyFunction agents

// Call to update reward for all agents
let updateRewardsForEveryRuleForAgent (agents : Agent list) (state : WorldState) : Agent list =
    List.map (updateReward state) agents

// Call to update social good for all current rules
let updateSocialGoodForEveryCurrentRule (agents : Agent list) (state : WorldState) : WorldState =
    let socialGood = median (List.map (fun agent -> (float)agent.Gain - 2.0 * agent.EnergyConsumed - 2.0 * agent.EnergyDeprecation) agents)
    let updatedRules = List.map (fun (x, y, oldSocialGood, lastUpdate) ->
        if lastUpdate = 0
        then (x, y, socialGood, 1)
        else
            (x, y, (oldSocialGood * (float)lastUpdate + socialGood) / (float)(lastUpdate + 1), lastUpdate + 1)) state.CurrentRuleSet
    {state with CurrentRuleSet = updatedRules}