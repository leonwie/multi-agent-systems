module Opinion
open Config
open Types
open Decision

// Computes median of a sequence    
let private median list = 
    let sorted = list |> Seq.toArray |> Array.sort
    let firstHalf, secondHalf = 
        let length = sorted.Length - 1 |> float
        length / 2. |> floor |> int, length / 2. |> ceil |> int 
    (sorted.[firstHalf] + sorted.[secondHalf] |> float) / 2.

// Calculated g_i aka g_agent where i = agent.ID
let private g (agent : Agent) : float = max (1.0 - 1.2 * agent.Susceptibility) 0.0

let private findAgentByID (id : int) (agents : Agent list) : Agent =
    List.filter (fun agent -> agent.ID = id) agents |> List.head
    
let private computePartialSums (agent : Agent) (agents : Agent list) (index : int) : (Rule * float) list=
    let otherAgent = findAgentByID index agents
    let x_in = otherAgent.DecisionOpinions.Value.PersonalCurrentRulesOpinion
    let a_in = List.filter (fun (x, _) -> x.ID = otherAgent.ID)  agent.DecisionOpinions.Value.AllOtherAgentsOpinion |> List.head |> snd
    List.map (fun (y, value) -> (y, value * a_in)) x_in
    
let private updateRuleOpinionPerAgent (agents : Agent list) (agent : Agent) : Agent =
    // g * X(0)
    let initialRuleOpinions = List.map (fun (rule, opinion) -> (rule, g agent * opinion)) agent.DecisionOpinions.Value.InitialRuleOpinion
    let partialSums = List.collect (fun index -> computePartialSums agent agents index)[0..numAgents - 1]
    let reduced = partialSums |> List.groupBy fst |> List.map (fun (x,y) -> x, (List.sum (List.map snd y)))
    let ruleChange =  List.map (fun (rule, value) -> (rule, (1.0 - g agent) * value)) reduced
    let sum = initialRuleOpinions @ ruleChange
    let updatedRuleOpinion = sum |> List.groupBy fst |> List.map (fun (x,y) -> x, (List.sum (List.map snd y)))
    let updatedDecision = {agent.DecisionOpinions.Value with PersonalCurrentRulesOpinion = updatedRuleOpinion}
    {agent with DecisionOpinions = Some updatedDecision}

let private updateRewardPerRule (agent : Agent) (newRewardForAgent : float) (rule : Rule) : (Rule * float * LastUpdate) =
    let rewards = agent.DecisionOpinions.Value.RewardPerRule
    let (ruleC, currentReward, lastUpdate) = List.filter (fun (currRule, _, _) -> rule = currRule) rewards |> List.head
    match lastUpdate with
    | 0 -> (ruleC, newRewardForAgent, 1)
    | t -> (ruleC, (currentReward * (float)t + newRewardForAgent) / (float)(t + 1), t + 1)
    
let private updateReward (state : WorldState) (agent : Agent) : Agent =
    let newRewardForAgent = (float) agent.Gain - agent.EnergyConsumed - agent.EnergyDeprecation
    let currentRules = List.map (fun (rules, _, _) -> rules) state.CurrentRuleSet
    let updatedRewards = List.map (updateRewardPerRule agent newRewardForAgent) currentRules
    let filteredRewards = List.filter (fun (rule, _, _) -> not(List.contains rule currentRules)) agent.DecisionOpinions.Value.RewardPerRule
    let updatedDecision = {agent.DecisionOpinions.Value with RewardPerRule = filteredRewards @ updatedRewards}
    {agent with DecisionOpinions = Some updatedDecision}

// Returns agents with modified agent RuleOpinion = G * X(0) + (I - G) * A * X(t) - sec 3.2.1 in Overleaf  
let updateRuleOpinion (agents : Agent list) : Agent list =
    let applyFunction agent = updateRuleOpinionPerAgent agents agent
    List.map applyFunction agents

// Call to update reward for all agents - sec 3.2.2 in Overleaf
let updateRewardsForEveryRuleForAgent (state : WorldState) (agents : Agent list) : Agent list =
    List.map (updateReward state) agents

// Call to update social good for all current rules - sec 3.2.3 in Overleaf
let updateSocialGoodForEveryCurrentRule (agents : Agent list) (state : WorldState) : WorldState =
    let socialGood = median (List.map (fun agent -> (float)agent.Gain - 2.0 * agent.EnergyConsumed - 2.0 * agent.EnergyDeprecation) agents)
    let updatedRules = List.map (fun (y, oldSocialGood, lastUpdate) ->
        if lastUpdate = 0 then
            (y, socialGood, 1)
        else
            (y, (oldSocialGood * (float)lastUpdate + socialGood) / (float)(lastUpdate + 1), lastUpdate + 1)) state.CurrentRuleSet
    //printf "updatedRules %A" updatedRules
    {state with CurrentRuleSet = updatedRules}

let private normaliseTheAgentArraysPerAgent (agent : Agent) : Agent =
    let rewardList = standardize (List.map (fun (_, reward, _) -> reward) agent.DecisionOpinions.Value.RewardPerRule)
    let restList = List.map (fun (rule, _, time) -> (rule, time)) agent.DecisionOpinions.Value.RewardPerRule
    let updatedPayoff = List.map (fun ((rule, time), reward) -> (rule, reward, time)) (List.zip restList rewardList) 
    let rewList = standardize (List.map (fun (_, reward) -> reward) agent.DecisionOpinions.Value.PersonalCurrentRulesOpinion)
    let ruleList = List.map (fun (rule, _) -> rule) agent.DecisionOpinions.Value.PersonalCurrentRulesOpinion
    let updatedCurrentRules = List.zip ruleList rewList
    let opinionList = standardize (List.map (fun(_, opinion) -> opinion) agent.DecisionOpinions.Value.AllOtherAgentsOpinion)
    let agentList = List.map (fun(agent, _) -> agent) agent.DecisionOpinions.Value.AllOtherAgentsOpinion
    let updatedAllOpinion = List.zip agentList opinionList
    let updatedDecision = {agent.DecisionOpinions.Value with RewardPerRule = updatedPayoff; PersonalCurrentRulesOpinion = updatedCurrentRules; AllOtherAgentsOpinion = updatedAllOpinion}
    {agent with DecisionOpinions = Some updatedDecision}

// sec 3.3 in Overleaf    
let normaliseTheAgentArrays (agents : Agent list) : Agent list =
    List.map normaliseTheAgentArraysPerAgent agents 
// sec 3.3 in Overleaf
let normaliseTheSocialGood (state : WorldState) : WorldState =
    let socialGoodList = List.map (fun (_, socialGood, _) -> socialGood) state.CurrentRuleSet
    let restList = List.map (fun (rule, _, time) -> (rule, time)) state.CurrentRuleSet
    let normalisedSocialGood = standardize socialGoodList
    let updatedSocialGood = List.map (fun ((rule, time), socialGood) -> (rule, socialGood, time)) (List.zip restList normalisedSocialGood) 
    {state with CurrentRuleSet = updatedSocialGood}
    
let private updateAggregationArray (state : WorldState) (agent : Agent)  : Agent =
    let susceptibilityMetric = List.map (fun (rule, opinion) -> (rule, opinion * agent.Susceptibility)) agent.DecisionOpinions.Value.PersonalCurrentRulesOpinion
    let rewardMetric = List.map (fun (rule, reward) -> (rule, reward * agent.Egotism)) (List.map (fun(rule, value, _) -> (rule, value)) agent.DecisionOpinions.Value.RewardPerRule)
    let socialGoodMetric = List.map (fun (rule, socialGood) -> (rule, socialGood * agent.Idealism)) (List.map (fun (rule, sGood, _) -> (rule, sGood)) state.AllRules)
    let concatenatedMetrics = susceptibilityMetric @ rewardMetric @ socialGoodMetric
    let updatedOverallRuleOpinion = concatenatedMetrics |> List.groupBy fst |> List.map (fun (x,y) -> x, (List.sum (List.map snd y)))
    let updatedDecision = {agent.DecisionOpinions.Value with OverallRuleOpinion = updatedOverallRuleOpinion}
    {agent with DecisionOpinions = Some updatedDecision}
    
// Call to generate O for every agent - sec 3.4 in Overleaf
let updateAggregationArrayForAgent (state : WorldState) (agents : Agent list)  : Agent list =
    List.map (updateAggregationArray state) agents

let private updateOpinion (agent : Agent) (rewardPerDay : float) (averageReward : float) (otherAgentOpinion : (Agent * float)) : (Agent * float)=
    let opinion = otherAgentOpinion|> snd
    let newOpinion =
        if rewardPerDay > averageReward then
            opinion + agent.Susceptibility * ((rewardPerDay - averageReward) / averageReward) * (1.0 - opinion) 
        else
            opinion * (1.0 - agent.Susceptibility * ((averageReward - rewardPerDay) / averageReward))
    (otherAgentOpinion |> fst, newOpinion)        

let private updateWorkingAgentsOpinion (state : WorldState) (agent : Agent) (workingAgents : (Agent * float) list) : (Agent * float) list =
    let huntingAgents = List.filter (fun (ag, _) -> ag.TodaysActivity |> fst = HUNTING) workingAgents
    let buildingAgents = List.filter (fun (ag, _) -> ag.TodaysActivity |> fst = BUILDING) workingAgents
    let huntingList = List.map (updateOpinion agent state.HuntingRewardPerDay state.HuntingAverageTotalReward) huntingAgents
    let buildingList = List.map (updateOpinion agent state.BuildingRewardPerDay state.BuildingAverageTotalReward) buildingAgents
    huntingList @ buildingList

let private updateNonWorking (agent : Agent) (otherAgent : Agent * float) : (Agent * float) =
    let opinion = otherAgent |> snd
    let newOpinion =
        if (otherAgent |> fst).Energy < agent.Energy then
            opinion
        else
            opinion * (1.0 - agent.Susceptibility * (((otherAgent |> fst).Energy - agent.Energy) / agent.Energy))
    (otherAgent |> fst, newOpinion)

let private updateNonWorkingAgentsOpinion (agent : Agent) (nonWorkingAgents : (Agent * float) list) : (Agent * float) list=
    List.map (updateNonWorking agent) nonWorkingAgents
    
let private updateWorkOpinionAdjustment (state : WorldState) (agent : Agent)  : Agent =
    let agentsOpinions = agent.DecisionOpinions.Value.AllOtherAgentsOpinion;
    let workingAgents = List.filter (fun (ag, _) -> not(ag.TodaysActivity |> fst = NONE)) agentsOpinions
    let nonWorkingAgents = List.filter (fun (ag, _) -> ag.TodaysActivity |> fst = NONE) agentsOpinions
    let workingOpinion = updateWorkingAgentsOpinion state agent workingAgents
    let nonWorkingOpinion = updateNonWorkingAgentsOpinion agent nonWorkingAgents
    let updatedDecision = {agent.DecisionOpinions.Value with AllOtherAgentsOpinion = workingOpinion @ nonWorkingOpinion}
    {agent with DecisionOpinions = Some updatedDecision}
    
// Ollie's part in the Agent-Decision-Making doc - for voting    
let votingOpinionAdjustment (agent : Agent) (otherAgent : Agent) (currentRule : Rule) (proposedRule : Rule) (voteA : bool) (voteO : bool) : (Agent * float) =
    let currentOpinion = List.filter (fun (ag, _) -> ag = otherAgent)  agent.DecisionOpinions.Value.AllOtherAgentsOpinion |> List.head |> snd
    let ruleOpinion rule = List.filter (fun (ru, _) -> ru = rule)  agent.DecisionOpinions.Value.OverallRuleOpinion |> List.head |> snd
    let newOpinion =
        if voteA = voteO then
            currentOpinion + (1.0 - currentOpinion) * abs (ruleOpinion currentRule - ruleOpinion proposedRule) * agent.Egotism 
        else
            currentOpinion * (1.0 - abs (ruleOpinion currentRule - ruleOpinion proposedRule) * agent.Egotism)
    (otherAgent, newOpinion)
    
// Ollie's part in the Agent-Decision-Making doc - for work
let workOpinions (state : WorldState) (agents : Agent List) : Agent list =
    List.map (updateWorkOpinionAdjustment state) agents
    
// Ollie's part in the Agent-Decision-Making doc - for self-confidence    
let selfConfidenceUpdate (agents : Agent list) : Agent list =
    let energySum = List.sum (List.map (fun agent -> agent.Energy) agents)
    let relativeWellbeing energy = energy / (energySum / (float)numAgents)
    let listWellbeing = List.map relativeWellbeing (List.map (fun agent -> agent.Energy) agents)
    let normalised = standardize listWellbeing
    let zipped = List.zip agents normalised
    let updatedSelfConfidence = List.map (fun (agent, relativeWellb) -> (agent, 0.5 * agent.Egotism + 0.5 * relativeWellb)) zipped
    List.map (fun (agent, selfConfidence) -> {agent with SelfConfidence = selfConfidence}) updatedSelfConfidence
    