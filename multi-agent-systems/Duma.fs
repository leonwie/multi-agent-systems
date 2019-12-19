module Duma

// TO DO: 
// CURRENT IMPLEMENTATION ASSUMES THAT THERE WILL BE PROPOSALS FOR ALL RULES, NEED TO CHANGE IT TO AN OPTION TYPE
// ANDREI NEEDS TO IMPLEMENT THE DECISION MAKING STUFF


open Types
open WorldState
open Voting

// Placeholders for decision making
let decisionMake1 (world : WorldState) (agents : Agent list) : Agent list = 
    failwithf "PLACEHOLDER FOR CHAIR (currentWorld.CurrentChair) TO DECIDE WHO CAN PROPOSE RULE CHANGES"


let decisionMake2 (agent : Agent) (world : WorldState) : (Rule * Agent) list = 
    failwithf "PLACEHOLDER FOR DECIDING WHAT PROPOSITIONS (shelter, food, work, voting system, max sanction) TO PROPOSE"


let decisionMake3 (world : WorldState) (agent : Agent) : Agent list =
    failwithf "PLACEHOLDER FOR DESCIDING ON NEW CHAIRMAN"


let decisionMake4 (world : WorldState) (agent : Agent)
    (toVote : ShelterRule list * WorkAllocation list * FoodRule list * VotingSystem list * Punishment list) 
    : ShelterRule list * WorkAllocation list * FoodRule list * VotingSystem list * Punishment list =
    failwithf "PLACEHOLDER FOR DECIDING WHAT RULES THE AGENT WILL VOTE FOR"

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


let propositions (agents : Agent list)  =
    // Get all the agents and the proposals they want to make, filter those allowed to make proposals
    let propositions =
        agents
        |> decisionMake1 currentWorld // Chair decides which agents can vote
        |> List.collect (fun el -> decisionMake2 el currentWorld)
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


let chairVote (agents : Agent list) (currentWorld : WorldState) : WorldState =
    // Vote on the new chair person
    if currentWorld.TimeToNewChair = 0 // Only change chaiman if necessary
    then // Get the opinions of each agent and carry out a vote on the chairman
        let newChair =
           agents
           |> List.map (decisionMake3 currentWorld) 
           |> match currentWorld.CurrentVotingRule with
               | Borda -> bordaVote
               | Approval -> approvalVote
               | InstantRunoff -> instantRunoffVote agents
               | Plurality -> pluralityVote
        {currentWorld with CurrentChair = Some newChair; TimeToNewChair = 7}
    else {currentWorld with TimeToNewChair = currentWorld.TimeToNewChair - 1}


let newRules (proposals : Proposal list) (agents : Agent list) : ShelterRule * WorkAllocation * FoodRule * VotingSystem * Punishment =
    // Get the rules to vote on
    let rulesToVoteOn =
        proposals
        |> List.fold (fun (acc : ShelterRule list * WorkAllocation list * FoodRule list * VotingSystem list * Punishment list) el -> 
            let a, b, c, d, e = 
                (fun (a, b, c, d, e) -> a, b, c, d, e) acc
            match el |> fst with
            | Shelter(x) -> 
                a @ [x], b, c, d, e
            | Work(x) -> 
                a, b @ [x], c, d, e
            | Food(x) -> 
                a, b, c @ [x], d, e
            | Voting(x) -> 
                a, b, c, d @ [x], e
            | Sanction(x) -> 
                a, b, c, d, e @ [x]
        ) ([], [], [], [], [])
    // Get the agent votes
    let agentVotes =
        agents
        |> List.map (fun agent -> 
            rulesToVoteOn
            |> fun (a, b, c, d, e) -> a, b, c, d, e
            |> decisionMake4 currentWorld agent)
        |> List.fold (fun acc el ->
            let acc1, acc2, acc3, acc4, acc5 = 
                (fun (a, b, c, d, e) -> a, b, c, d, e) acc
            let el1, el2, el3, el4, el5 = 
                (fun (a, b, c, d, e) -> [a], [b], [c], [d], [e]) el
            acc1 @ el1,
            acc2 @ el2,
            acc3 @ el3,
            acc4 @ el4,
            acc5 @ el5
        ) ([], [], [], [], [])
    // Apply current voting system to each of the four voting options
    agentVotes
    |> fun (a, b, c, d, e) -> 
        let votingSystem (allCandidates : 'a list) (candidates : 'a list list) =
            match currentWorld.VotingType with
            | Borda -> 
                bordaVote candidates
            | Approval -> 
                approvalVote candidates
            | Plurality -> 
                pluralityVote candidates
            | InstantRunoff -> 
                instantRunoffVote allCandidates candidates
        votingSystem allShelterRules a, 
        votingSystem allWorkRules b, 
        votingSystem allFoodRules c, 
        votingSystem allVotingSystems d,
        votingSystem allSanctionVotes e
        

let RuleImplentation (rulesToImplement : ShelterRule * WorkAllocation * FoodRule * VotingSystem * Punishment) : WorldState =
    // Implement the new rules
    let newShelterRule, newWorkRule, newFoodRule, newVotingSystem, newSanction = 
        rulesToImplement
        |> fun (a, b, c, d, e) -> a, b, c, d, e
    // Get all new stuff except sanctions
    let newWorld = {
        currentWorld with
            CurrentShelterRule = newShelterRule;
            CurrentWorkRule = newWorkRule;
            CurrentFoodRule = newFoodRule;
            CurrentVotingRule = newVotingSystem;
        }
    // Punishment can be either incr or decr or changing max punishment   
    match newSanction with
    | x when x = Increment -> 
        {newWorld with CurrentSactionStepSize = newWorld.CurrentSactionStepSize + 0.1}
    | x when x = Decrement -> 
        {newWorld with CurrentSactionStepSize = newWorld.CurrentSactionStepSize - 0.1}
    | _ -> 
        {newWorld with CurrentMaxPunishment = newSanction}
