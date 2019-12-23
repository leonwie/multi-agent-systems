module Duma

open Types
open Voting
open Config
open Election

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


let voteOnProposals (world : WorldState) (agent : Agent)
    (toVote : ShelterRule list option * WorkAllocation list option * FoodRule list option * VotingSystem list option * Punishment list option)
    : ShelterRule list option * WorkAllocation list option * FoodRule list option * VotingSystem list option * Punishment list option =
    // Stuff to vote on will either be a list of values or a None if noone proposed a proposal change
    failwithf "PLACEHOLDER FOR DECIDING WHAT RULES THE AGENT WILL VOTE FOR"
    
(*
let voteOnChairCandidates (agent : Agent) (allCandidates : Agent list) : Agent list =
    match agent.DecisionOpinions with
        | Some(opinions) -> opinions.OtherAgentsOpinion
        | None -> failwithf "Should have opinions of other agents"
    |> List.sortBy snd // sort list by size of opinion
    |> List.rev // Get largest to smallest
    |> List.filter (fun el -> List.contains (el |> fst) allCandidates) // filter by elements only in both lists
    |> List.map fst // map agent * float to agent    
*)

let getPropositions (world : WorldState) (agents : Agent list) : Proposal list =
    // Get all the agents and the proposals they want to make, filter those allowed to make proposals
    let propositions =
        agents
        |> List.collect (fun agent -> proposalOfRuleChangesForAgent agent world) // Gets a list of rules to propose and agents
        (* // Replace this with stuff that matches your implementation
        |> chairChoiceOfProposalForTopic
        |> doesTheChairUseVeto
        
        
        |> List.fold (fun acc el ->
            match el with // Filter out None types so that only the new rule propositions remain
            | Some(x), l -> acc @ [x, l]
            | None, _ -> acc
        ) []  *)
        
        
    // Turn from an (Agent * Rule list) list to an (Rule * Agent list) list (e.g. Proposal list)
    propositions
    |> List.fold (fun (acc : Proposal list) el ->
        let rule = el |> fst
        if List.contains rule (List.map fst acc)
        then // This whole function can probably be implemented better...
            let index = List.findIndex (fun el -> el |> fst = rule) acc
            List.mapi (fun i el1 ->
                if i = index
                then rule, (el1 |> snd) @ [el |> snd]
                else el1) acc
        else acc @ [el |> fst, [el |> snd]]
    ) []


let chairVote (world : WorldState) (agents : Agent list) : WorldState =
    // Vote on the new chair person
    if world.TimeToNewChair = 0 // Only change chaiman if necessary
    then // Get the opinions of each agent and carry out a vote on the chairman
        let candidates = 
            agents // Does an agent nominate itself
            |> List.filter (fun agent -> doesAgentNominateItselfForChair agent world.RuleSet nominationThreshold)
        let newChair =
            agents
            |> List.map (fun agent -> agentVoteForChair candidates agent)
            |> match world.CurrentVotingRule with
               | Borda -> bordaVote
               | Approval -> approvalVote
               | InstantRunoff -> instantRunoffVote agents
               | Plurality -> pluralityVote
        {world with CurrentChair = Some newChair; TimeToNewChair = 7}
    else {world with TimeToNewChair = world.TimeToNewChair - 1}


let newRules (agents : Agent list) (world : WorldState) (proposals : Proposal list) : ShelterRule option * WorkAllocation option * FoodRule option * VotingSystem option * Punishment option =
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
                        | None -> failwithf "This shouldn't happen since the List should either be all None (already dealt with) or all Some"
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
            |> voteOnProposals world agent)
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


let implementNewRules (world : WorldState) (rulesToImplement : ShelterRule option * WorkAllocation option * FoodRule option * VotingSystem option * Punishment option) : WorldState =
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
            CurrentSanctionStepSize = newWorld.CurrentSanctionStepSize + 0.1}
    | x when x = Some(Decrement) ->
        {newWorld with
            CurrentSanctionStepSize = newWorld.CurrentSanctionStepSize - 0.1}
    | _ -> // If not Some(increment) or Some(decrement) then must be a max sanction update or None
        {newWorld with
            CurrentMaxPunishment =
                applyOptionRule newSanction world.CurrentMaxPunishment
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