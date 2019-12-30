module Election
open Config
open Types

// There are 5 rule types: Food, Shelter, Punishment, Voting, Work alloc
let private numberOfRuleTypes = 5;

// Find a value given a key in a tuple list
let rec private lookUpInTuple tupleList toFind =
    match tupleList with
    | [] -> None
    | (x, y) :: _ when x.ID = toFind.ID -> Some y
    | _ :: tail -> lookUpInTuple tail toFind

// Intersection of two sorted lists
let rec private intersect xs ys =
    match xs, ys with
    | x::xs', y::ys' ->
        if   x = y then x :: intersect xs' ys'
        elif x < y then intersect xs' ys
        else            intersect xs  ys'
    | _ -> []
    
// n_i= (μ_Ri +(size(k))/(∑_k s_k )∙μ_Si)∙ a_ii  k = set of rules currently in use 
let doesAgentNominateItselfForChair (agent : Agent) (ruleSet : RuleSet) (threshold : float): bool =
    let a_ii = agent.SelfConfidence
    let likelihood = agent.Egotism + agent.Susceptibility * ((float)numberOfRules /
                                            List.sum (List.map(fun (_, socialGood, _) -> socialGood) ruleSet)) * a_ii
    likelihood > threshold                                        

// Return a sorted desc list of candidate preferences for 'agent'    
let agentVoteForChair (candidates : Agent list) (agent : Agent) : Agent list =
    let opinions = agent.DecisionOpinions.Value.AllOtherAgentsOpinion 
    let orderedAgentPreference = List.map fst (List.sortBy (fun (_, y) -> -y) opinions)
    List.filter (fun el -> not(List.contains el candidates)) orderedAgentPreference

let private checkOptionsPastRules (rules : Rule list)  (pastOpinions : (Rule * float) list) (currentRule : Rule) (currentOpinion : float) : (Rule * float) =
    let pastRules = List.filter (fun (rule, _) -> rule <> currentRule && List.contains rule rules) pastOpinions
    let (_, minOpinion) = List.sortBy (fun (_, y) -> -y) pastOpinions |> List.head
    if (minOpinion >= currentOpinion) then
        List.sortBy (fun (_, y) -> -y) pastRules |> List.head
    else (currentRule, -1.0)    
    
let private checkOpinionsPastRulesInTopic (pastOpinions : (Rule * float) list) (currentRule : Rule) (currentOpinion : float) : (Rule * float) =
    match currentRule with
    | Shelter(x) -> checkOptionsPastRules ShelterRuleList pastOpinions currentRule  currentOpinion
    | Food(x) -> checkOptionsPastRules FoodRuleList pastOpinions currentRule currentOpinion
    | Voting(x) -> checkOptionsPastRules VotingSystemList pastOpinions currentRule currentOpinion
    | Work(x) -> checkOptionsPastRules WorkAllocationList pastOpinions currentRule currentOpinion
    | Sanction(x) -> checkOptionsPastRules PunishmentList pastOpinions currentRule currentOpinion
    
let private proposalOfRuleChangesForOneTopic (agent : Agent) (currentRule : Rule) : (Rule * float) =
    let (_, currentOpinionOnRule) = List.filter (fun (rule, _) -> currentRule = rule) agent.DecisionOpinions.Value.OverallRuleOpinion |> List.head
    let (pastRule, num) = checkOpinionsPastRulesInTopic agent.DecisionOpinions.Value.OverallRuleOpinion currentRule currentOpinionOnRule
    if num > currentOpinionOnRule then (pastRule, num - currentOpinionOnRule) else (currentRule, 0.0)

// Returns a Rule list with either 1 element or empty - run this for every agent
let proposalOfRuleChangesForAgent (agent : Agent) (state : WorldState) : (Agent * Rule list) =
    let currentRules = List.map (fun (rule, _, _) -> rule) state.CurrentRuleSet
    let scores = List.map (proposalOfRuleChangesForOneTopic agent) currentRules
    let ruleProposals = List.filter (fun (_, y) -> y <> 0.0) scores
    match List.length ruleProposals with
    | 0 -> (agent, [])
    | 1 -> (agent, [ruleProposals |> List.head |> fst])
    | _ -> (agent, [List.sortBy (fun (_, y) -> -y) ruleProposals |> List.head |> fst])
    
// F_j=a_ij∙μ_Xi+O_ik∙a_ii
let private choiceOfProposalForOneAgent (chair : Agent) (currentRule : Rule) (agentProposingRule : Agent) : float=
    let a_ii = chair.SelfConfidence
    let a_ij = (lookUpInTuple chair.DecisionOpinions.Value.AllOtherAgentsOpinion agentProposingRule).Value
    let (_, currentOpinionOnRule) = List.filter (fun (rule, _) -> currentRule = rule) chair.DecisionOpinions.Value.OverallRuleOpinion |> List.head
    a_ij * chair.Susceptibility + a_ii * currentOpinionOnRule

// Returns agent allowed to speak for one topic (need to call this method 5 times for the diff 5 currentRules)
let chairChoiceOfProposalForTopic (chair : Agent) (agentsProposingRulesInTopic : Agent list) (currentRule : Rule) : Agent list =
    if agentsProposingRulesInTopic.Length = 0 then
        []
    else
        let fForAgentsInTopic = List.zip agentsProposingRulesInTopic (List.map (choiceOfProposalForOneAgent chair currentRule) agentsProposingRulesInTopic)
        [List.map fst (List.sortBy (fun (_, y) -> -y) fForAgentsInTopic) |> List.head]
    
// Returns true is chair vetoes the new rule    
let doesTheChairUseVeto (chair : Agent) (agentProposingRule : Agent)
                        (currentRule : Rule) (newRule : Rule) (threshold : float) : bool =
    let (_, oldRuleOpinion) = List.filter (fun (rule, _) -> rule = currentRule) chair.DecisionOpinions.Value.OverallRuleOpinion |> List.head
    let (_, newRuleOpinion) = List.filter (fun (rule, _) -> rule = newRule) chair.DecisionOpinions.Value.OverallRuleOpinion |> List.head
    let opinionDifferenceBetweenRuleChange = oldRuleOpinion - newRuleOpinion
    let a_ii = chair.SelfConfidence
    let a_ij = (lookUpInTuple chair.DecisionOpinions.Value.AllOtherAgentsOpinion agentProposingRule).Value
    let b_t = 1.0 - opinionDifferenceBetweenRuleChange * a_ii - a_ij * chair.Susceptibility
    b_t > threshold
