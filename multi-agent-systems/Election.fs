module Election
open Config
open Types

// There are 5 rule types: Food, Shelter, Punishment, Voting, Work alloc
let private numberOfRuleTypes = 5;

// Find a value given a key in a tuple list
let rec private lookUpInTuple tupleList toFind =
    match tupleList with
    | [] -> None
    | (x, y) :: _ when x.ID = toFind -> Some y
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
let doesAgentNominateItselfForChair (agent : Agent) (ruleSet : RuleSet) (threshold : float) : bool =
    let a_ii = (lookUpInTuple agent.DecisionOpinions.Value.OtherAgentsOpinion agent.ID).Value
    let likelihood = agent.Egotism + agent.Susceptibility * ((float)numberOfRules /
                                            List.sum (List.map(fun (_, _, socialGood) -> socialGood) ruleSet)) * a_ii
    likelihood > threshold                                        

// Return a sorted desc list of candidate preferences for 'agent'    
let agentVoteForChair (candidates : Agent list) (agent : Agent) : Agent list =
    let opinions = agent.DecisionOpinions.Value.OtherAgentsOpinion 
    let orderedAgentPreference = List.map fst (List.sortBy (fun (_, y) -> -y) opinions)
    intersect orderedAgentPreference (candidates |> List.sortDescending)

let private checkOpinionsPastRulesInTopic (pastOpinions : (RuleTypes * Rule * float) list) (ruleType : RuleTypes) (currentRule : Rule) : (RuleTypes * Rule * float) =
    let pastRules = List.filter (fun (ruleT, rule, _) -> ruleT = ruleType && rule <> currentRule) pastOpinions
    List.sortBy (fun (_, _, y) -> -y) pastRules |> List.head
    
let private proposalOfRuleChangesForOneTopic (agent : Agent) (currentRule : Rule) (ruleType : RuleTypes) : (Rule * float) =
    let currentOpinionOnRule = agent.DecisionOpinions.Value.CurrentRulesOpinion.[(int)ruleType]
    let (_, pastRule, num) = checkOpinionsPastRulesInTopic agent.DecisionOpinions.Value.PastRulesOpinion ruleType currentRule
    if num > currentOpinionOnRule then (pastRule, num - currentOpinionOnRule) else (currentRule, 0.0)

// Returns a Rule list with either 1 element or empty - run this for every agent
let proposalOfRuleChangesForAgent (agent : Agent) (state : WorldState) : Rule list=
    let currentRules = List.map (fun (rule, _, _) -> rule) state.RuleSet
    let scores = List.map (fun ruleType -> proposalOfRuleChangesForOneTopic agent currentRules.[(int)ruleType] ruleType)[RuleTypes.SHELTER; RuleTypes.FOOD; RuleTypes.WORK; RuleTypes.VOTING; RuleTypes.SANCTION]
    let ruleProposals = List.filter (fun (x, y) -> y <> 0.0) scores
    match List.length ruleProposals with
    | 0 -> []
    | 1 -> [ruleProposals |> List.head |> fst]
    | _ -> [List.sortBy (fun (_, y) -> -y) ruleProposals |> List.head |> fst]
    
// F_j=a_ij∙μ_Xi+O_ik∙a_ii
let private choiceOfProposalForOneAgent (chair : Agent) (currentRuleIndex : RuleTypes) (agentProposingRule : Agent) : float=
    let a_ii = (lookUpInTuple chair.DecisionOpinions.Value.OtherAgentsOpinion chair.ID).Value
    let a_ij = (lookUpInTuple chair.DecisionOpinions.Value.OtherAgentsOpinion agentProposingRule.ID).Value
    let opinionOnRule = chair.DecisionOpinions.Value.CurrentRulesOpinion.[(int)currentRuleIndex]
    a_ij * chair.Susceptibility + a_ii * opinionOnRule

// Returns agent allowed to speak for one topic (need to call this method 5 times for the diff 5 topics)    
let chairChoiceOfProposalForTopic (chair : Agent) (agentsProposingRulesInTopic : Agent list) (currentRuleIndex : RuleTypes) : Agent =
    let fForAgentsInTopic = List.zip agentsProposingRulesInTopic (List.map (choiceOfProposalForOneAgent chair currentRuleIndex) agentsProposingRulesInTopic)
    List.map fst (List.sortBy (fun (_, y) -> -y) fForAgentsInTopic) |> List.head
    
// Returns true is chair vetoes the new rule    
let doesTheChairUseVeto (chair : Agent) (agentProposingRule : Agent)
                        (oldRuleOpinion : float) (newRuleOpinion : float) (threshold : float) : bool =
    let opinionDifferenceBetweenRuleChange = oldRuleOpinion - newRuleOpinion
    let a_ii = (lookUpInTuple chair.DecisionOpinions.Value.OtherAgentsOpinion chair.ID).Value
    let a_ij = (lookUpInTuple chair.DecisionOpinions.Value.OtherAgentsOpinion agentProposingRule.ID).Value
    let b_t = 1.0 - opinionDifferenceBetweenRuleChange * a_ii - a_ij * chair.Susceptibility
    b_t > threshold
