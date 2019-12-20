module Duma

// TO DO: 
// CURRENT IMPLEMENTATION ASSUMES THAT THERE WILL BE PROPOSALS FOR ALL RULES, NEED TO CHANGE IT TO AN OPTION TYPE
// NEED TO ADD THE SANCTION STUFF OR WHATEVER IT ENDS UP BEING
// NEED TO IMPLEMENT THE DECISION MAKING STUFF
// NEED TO IMPLEMENT FOOD DISTRIBUTION, WORK DISTRIBUTION, SHELTER DISTRIBUTION, AND ENERGY TO PUT IN ETC. ACCORDING TO THESE RULES


open Types
open Voting

// Placeholders for decision making
let decisionMake1 (world : WorldState) (agents : Agent list) : Agent list = 
    failwithf "PLACEHOLDER FOR CHAIR (currentWorld.CurrentChair) TO DECIDE WHO CAN PROPOSE RULE CHANGES"


let decisionMake2 (agent : Agent) (world : WorldState) : (Rule * Agent) list = 
    failwithf "PLACEHOLDER FOR DECIDING WHAT PROPOSITIONS TO PROPOSE"


let decisionMake3 (world : WorldState) (agent : Agent) : Agent list =
    failwithf "PLACEHOLDER FOR DESCIDING ON NEW CHAIRMAN"


let decisionMake4 (world : WorldState) (agent : Agent)
    (toVote : ShelterRule list * WorkAllocation list * FoodRule list * VotingSystem list) 
    : ShelterRule list * WorkAllocation list * FoodRule list * VotingSystem list =
    failwithf "PLACEHOLDER FOR DECIDING WHAT RULES THE AGENT WILL VOTE FOR"

// Need all possible rules in a list for instant runoff
let allShelterRules : ShelterRule list = [
    Random;
    Oligarchy;
    Meritocracy;
    Socialism
]

let allFoodRules : FoodRule list = [
    Communism;
    FoodRule.Socialism;
    FoodRule.Meritocracy;
    FoodRule.Oligarchy
]

let allWorkRules : WorkAllocation list = [
    ByChoice;
    Everyone;
    Strongest
]

let allVotingSystems : VotingSystem list = [
    Borda;
    Approval;
    InstantRunoff;
    Plurality
]


let propositions (agents : Agent list) (currentWorld : WorldState) =
    // Get all the agents and the proposals they want to make, filter those allowed to make proposals
    let propositions =
        agents
        |> decisionMake1 currentWorld
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


let newRules (proposals : Proposal list) (agents : Agent list) (currentWorld : WorldState) : ShelterRule * WorkAllocation * FoodRule * VotingSystem =
    // Get the rules to vote on
    let rulesToVoteOn =
        proposals
        |> List.fold (fun (acc : ShelterRule list * WorkAllocation list * FoodRule list * VotingSystem list) el -> 
            let a, b, c, d = (fun (a, b, c, d) -> a, b, c, d) acc
            match el |> fst with
            | Shelter(x) -> a @ [x], b, c, d
            | Work(x) -> a, b @ [x], c, d
            | Food(x) -> a, b, c @ [x], d
            | Voting(x) -> a, b, c, d @ [x]
        ) ([], [], [], [])
    // Get the agent votes
    let agentVotes =
        agents
        |> List.map (fun agent -> 
            rulesToVoteOn
            |> fun (a, b, c, d) -> a, b, c, d
            |> decisionMake4 currentWorld agent)
        |> List.fold (fun acc el ->
            let acc1, acc2, acc3, acc4 = (fun (a, b, c, d) -> a, b, c, d) acc
            let el1, el2, el3, el4 = (fun (a, b, c, d) -> [a], [b], [c], [d]) el
            acc1 @ el1,
            acc2 @ el2,
            acc3 @ el3,
            acc4 @ el4
        ) ([], [], [], [])
    // Apply current voting system to each of the four voting options
    agentVotes
    |> fun (a, b, c, d) -> 
        let votingSystem (allCandidates : 'a list) (candidates : 'a list list) =
            match currentWorld.CurrentVotingRule with
            | Borda -> bordaVote candidates
            | Approval -> approvalVote candidates
            | Plurality -> pluralityVote candidates
            | InstantRunoff -> instantRunoffVote allCandidates candidates
        votingSystem allShelterRules a, 
        votingSystem allWorkRules b, 
        votingSystem allFoodRules c, 
        votingSystem allVotingSystems d
        

let RuleImplentation (rulesToImplement : ShelterRule * WorkAllocation * FoodRule * VotingSystem) (currentWorld : WorldState): WorldState =
    // Implement the new rules
    let newShelterRule, newWorkRule, newFoodRule, newVotingSystem = 
        rulesToImplement
        |> fun (a, b, c, d) -> a, b, c, d
    {
        currentWorld with
            CurrentShelterRule = newShelterRule;
            CurrentWorkRule = newWorkRule;
            CurrentFoodRule = newFoodRule;
            CurrentVotingRule = newVotingSystem;
    }
