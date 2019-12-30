module Duma

open System
open System.Data
open Types
open Voting
open Config
open Decision
open Election
open Opinion

// To Do:
//
// Define Ruleset

// Need all possible rules in a list for instant runoff
let allShelterRules : ShelterRule list = [
    Random;
    Oligarchy;
    Meritocracy;
    Socialism;
]

let allFoodRules : FoodRule list = [
    Communism;
    FoodRule.Socialism;
    FoodRule.Meritocracy;
    FoodRule.Oligarchy;
]

let allWorkRules : WorkAllocation list = [
    ByChoice;
    Everyone;
    Strongest;
]

let allVotingSystems : VotingSystem list = [
    Borda;
    Approval;
    InstantRunoff;
    Plurality;
]

let allSanctionVotes : Punishment list = [
    NoFoodAndShelter;
    Exile;
    Increment;
    Decrement;
]

//let voteOnProposals (world : WorldState) (agent : Agent)
//    (toVote : ShelterRule list option * WorkAllocation list option * FoodRule list option * VotingSystem list option * Punishment list option)
//    : ShelterRule list option * WorkAllocation list option * FoodRule list option * VotingSystem list option * Punishment list option =
//    // Stuff to vote on will either be a list of values or a None if noone proposed a proposal change
//    match world.CurrentVotingRule with
//       | Borda -> bordaVote
//       | Approval -> approvalVote
//       | InstantRunoff -> instantRunoffVote agents
//       | Plurality -> pluralityVote
    
let private getAgentToSpeakForTopic (chair : Agent) (currentRuleInTopic : Rule) (newRules : (Agent * Rule) list) : Agent list =
    let filteredRules =
        match currentRuleInTopic with
        | Shelter(x) -> (List.filter (function (_, Shelter _) -> true | _ -> false) newRules) |> List.map fst
        | Food(x) ->  (List.filter (function (_, Food _) -> true | _ -> false) newRules) |> List.map fst
        | Work(x) -> (List.filter (function (_, Work _) -> true | _ -> false) newRules) |> List.map fst
        | Voting(x) -> (List.filter (function (_, Voting _) -> true | _ -> false) newRules) |> List.map fst
        | Sanction(x) -> (List.filter (function (_, Sanction _) -> true | _ -> false) newRules) |> List.map fst
    chairChoiceOfProposalForTopic chair filteredRules currentRuleInTopic
    
let getPropositions (world : WorldState) (agents : Agent list) : Proposal list =
    // Get all the agents and the proposals they want to make, filter those allowed to make proposals
    let proposalOfRuleChanges = List.map (fun agent -> proposalOfRuleChangesForAgent agent world) agents
    let agentNewRules = List.filter (fun (_, list) -> not(List.isEmpty list)) proposalOfRuleChanges
                        |> List.map (fun (agent, rules) -> (agent, List.head rules))
    let agentForTopic rule = getAgentToSpeakForTopic world.CurrentChair.Value rule agentNewRules
    let agentOldRules = world.CurrentRuleSet
                          |> List.filter (fun (rule, _, _) -> (agentForTopic rule).Length > 0)
                          |> List.map (fun (rule, _, _) -> (agentForTopic rule |> List.head, rule))
                          |> List.sort
    let newRules = List.filter (fun (agent, _) -> List.contains agent (List.map fst agentOldRules))
                       agentNewRules |> List.sort
    let zipped = List.zip agentOldRules newRules
    let updatedProposals = List.filter (fun ((agent, oldRule), (_, newRule)) ->
        not(doesTheChairUseVeto world.CurrentChair.Value agent oldRule newRule vetoThreshold)) zipped
    List.map (fun ((agent, _), (_, newRule)) -> (newRule, [agent])) updatedProposals
    
let chairVote (world : WorldState) (agents : Agent list) : WorldState =
    // Vote on the new chair person
    if world.TimeToNewChair = 0 || world.CurrentChair.IsNone // Only change chaiman if necessary
    then // Get the opinions of each agent and carry out a vote on the chairman
        let candidates = 
            agents // Does an agent nominate itself
            |> List.filter (fun agent -> doesAgentNominateItselfForChair agent world.CurrentRuleSet nominationThreshold)
        let newChair =
            agents
            |> List.map (fun agent -> agentVoteForChair candidates agent)
            |> match world.CurrentVotingRule with
               | Borda -> bordaVote
               | Approval -> approvalVote
               | InstantRunoff -> instantRunoffVoteA agents
               | Plurality -> pluralityVote
        let chair = List.filter (fun agent -> agent.ID = newChair.ID) agents |> List.head       
        {world with CurrentChair = Some chair; TimeToNewChair = 7}
    else {world with TimeToNewChair = world.TimeToNewChair - 1}


let newRules (agents : Agent list) (world : WorldState) (proposals : Proposal list) : ShelterRule option * WorkAllocation option
                                                            * FoodRule option * VotingSystem option * Punishment option =
    // Get the rules to vote on
    let rulesToVoteOn =
        proposals
        |> List.fold (fun (acc : ShelterRule list * WorkAllocation list * FoodRule list * VotingSystem list * Punishment list) el ->
            let acc1, acc2, acc3, acc4, acc5 = acc
            match el |> fst with
            | Shelter(x) ->
                acc1 @ [x], acc2, acc3, acc4, acc5
            | Work(x) ->
                acc1, acc2 @ [x], acc3, acc4, acc5
            | Food(x) ->
                acc1, acc2, acc3 @ [x], acc4, acc5
            | Voting(x) ->
                acc1, acc2, acc3, acc4 @ [x], acc5
            | Sanction(x) ->
                acc1, acc2, acc3, acc4, acc5 @ [x]
        ) ([], [], [], [], []) // Some of these might be empty so need to make option later
    // Get the agent votes
    let agentVotes =
        // Make empty lists into None
        let optionMake (list : 'a list) : 'a list option =
            match list with
            | [] -> None
            | l -> Some(l)
        // Since some lists will be None, we need a way of ignoring those lists when they are concatinated
        let removeNone (list : 'a list option list) : 'a list list option =
            match list with
            | x when List.contains None x -> None
            | x -> List.map (
                    fun el ->
                        match el with
                        | Some(y) -> y
                        | None -> failwithf "This shouldn't happen since the List should either be all None
                                                                                      (already dealt with) or all Some"
                    ) x |> Some
        let rulesToVoteOn1 =
            rulesToVoteOn
            |> fun (a, b, c, d, e) ->
                // Get rid of empty lists by setting them to None so decision making has an easier time
                optionMake a,
                optionMake b,
                optionMake c,
                optionMake d,
                optionMake e
        agents
        |> List.map (fun agent ->
            rulesToVoteOn1
            |> id)
        |> List.fold (fun acc el ->
            let acc1, acc2, acc3, acc4, acc5 = acc
            let el1, el2, el3, el4, el5 = el
            acc1 @ [el1],
            acc2 @ [el2],
            acc3 @ [el3],
            acc4 @ [el4],
            acc5 @ [el5]
        ) ([], [], [], [], [])
        |> fun (a, b, c, d, e) ->
            // Make a 'list option list' into a 'list list option'
            removeNone a,
            removeNone b,
            removeNone c,
            removeNone d,
            removeNone e
    // Apply current voting system to each of the voting options
    let votingSystem (allCandidates : 'a list) (candidates : 'a list list) =
        match world.CurrentVotingRule with
        | Borda ->
            bordaVote candidates
        | Approval ->
            approvalVote candidates
        | Plurality ->
            pluralityVote candidates
        | InstantRunoff ->
            instantRunoffVote allCandidates candidates
    // Vote can result in an option type if there is nothing to vote for, in that case we ignore it
    let vote (votingSystem) (allCandidates : 'a list) (candidates : 'a list list option) =
        match candidates with
        | Some(x) ->
            votingSystem allCandidates x
            |> Some
        | None -> None
    // Get the ballots
    let shelterVotes, workVotes, foodVotes, votingVotes, sanctionVotes = agentVotes
    // Calculate winner based on votes and return as a tuple
    vote votingSystem allShelterRules shelterVotes,
    vote votingSystem allWorkRules workVotes,
    vote votingSystem allFoodRules foodVotes,
    vote votingSystem allVotingSystems votingVotes,
    vote votingSystem allSanctionVotes sanctionVotes

let private setRuleSet (world : WorldState) (shelterRule : ShelterRule) (foodRule : FoodRule) (workRule : WorkAllocation)
                       (votingRule : VotingSystem) (sanctionRule : Punishment) : RuleSet =
    let isNewRule rule =
        match rule with
        | Shelter(x) -> x = shelterRule
        | Food(x) -> x = foodRule
        | Work(x) -> x = workRule
        | Voting(x) -> x = votingRule
        | Sanction(x) -> x = sanctionRule
    List.filter (fun (rule, _, _) -> isNewRule rule) world.AllRules

let private setAllRules (world : WorldState) : RuleSet =
    let currentRules = List.map (fun (rule, _, _) -> rule) world.CurrentRuleSet
    let allRulesMinusCurrent = List.filter (fun (rule, _, _) -> not(List.contains rule currentRules)) world.AllRules
    world.CurrentRuleSet @ allRulesMinusCurrent
    
let implementNewRules (world : WorldState) (rulesToImplement : ShelterRule option * WorkAllocation option *
                                            FoodRule option * VotingSystem option * Punishment option) : WorldState =
    // Implement the new rules
    let newShelterRule, newWorkRule, newFoodRule, newVotingSystem, newSanction = rulesToImplement
    // If rule is None due to no vote on it, set it to the old rule
    let applyOptionRule (newRule : 'a option) (oldRule : 'a) : 'a =
        match newRule with
        | Some(x) -> x
        | None -> oldRule
    // Get all new stuff except max sanctions
    let newWorld = {
        world with
            CurrentShelterRule = applyOptionRule newShelterRule world.CurrentShelterRule;
            CurrentWorkRule = applyOptionRule newWorkRule world.CurrentWorkRule;
            CurrentFoodRule = applyOptionRule newFoodRule world.CurrentFoodRule;
            CurrentVotingRule = applyOptionRule newVotingSystem world.CurrentVotingRule;
        }
    // Punishment can be either incr or decr or changing max punishment
    match newSanction with
    | x when x = Some(Increment) ->
        {newWorld with
            CurrentSanctionStepSize = newWorld.CurrentSanctionStepSize + 0.1;
            AllRules = setAllRules world;
            CurrentRuleSet = setRuleSet world (applyOptionRule newShelterRule world.CurrentShelterRule)
                                 (applyOptionRule newFoodRule world.CurrentFoodRule)
                                 (applyOptionRule newWorkRule world.CurrentWorkRule)
                                 (applyOptionRule newVotingSystem world.CurrentVotingRule) world.CurrentMaxPunishment;
        }
    | x when x = Some(Decrement) ->
        {newWorld with
            CurrentSanctionStepSize = newWorld.CurrentSanctionStepSize - 0.1;
            AllRules = setAllRules world;
            CurrentRuleSet = setRuleSet world (applyOptionRule newShelterRule world.CurrentShelterRule)
                                 (applyOptionRule newFoodRule world.CurrentFoodRule)
                                 (applyOptionRule newWorkRule world.CurrentWorkRule)
                                 (applyOptionRule newVotingSystem world.CurrentVotingRule) world.CurrentMaxPunishment;
        }
    | _ -> // If not Some(increment) or Some(decrement) then must be a max sanction update or None
        {newWorld with
            CurrentMaxPunishment = applyOptionRule newSanction world.CurrentMaxPunishment;
            AllRules = setAllRules world;
            CurrentRuleSet = setRuleSet world (applyOptionRule newShelterRule world.CurrentShelterRule)
                                 (applyOptionRule newFoodRule world.CurrentFoodRule)
                                 (applyOptionRule newWorkRule world.CurrentWorkRule)
                                 (applyOptionRule newVotingSystem world.CurrentVotingRule)    
                                 (applyOptionRule newSanction world.CurrentMaxPunishment);
        }


let fullDuma (agents : Agent list) (world : WorldState) : WorldState =
    // Do chair vote
    let newWorld =
        agents
        |> chairVote world
    // Apply all the duma stuff returning a worldstate containing the new ruleset
    agents
        |> getPropositions newWorld
        |> newRules agents newWorld
        |> implementNewRules newWorld
