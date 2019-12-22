module Duma
//
//open Types
//open Voting
//open Config
//
//// To Do:
//// Improve makeProposals
//// Improve voteOnProposals
//// Try and get rid of IndexToRule and ruleToIndex
//// Try and merge the rule lists into one
//// Implement socialGood Function
//// Actually test it...
//
//// Need all possible rules in a list for instant runoff
//let allShelterRules : ShelterRule list = [
//    Random;
//    Oligarchy;
//    Meritocracy;
//    Socialism;
//]
//
//let allFoodRules : FoodRule list = [
//    Communism;
//    FoodRule.Socialism;
//    FoodRule.Meritocracy;
//    FoodRule.Oligarchy;
//]
//
//let allWorkRules : WorkAllocation list = [
//    ByChoice;
//    Everyone;
//    Strongest;
//]
//
//let allVotingSystems : VotingSystem list = [
//    Borda;
//    Approval;
//    InstantRunoff;
//    Plurality;
//]
//
//let allSanctionVotes : Punishment list = [
//    NoFoodAndShelter;
//    Exile;
//    Increment;
//    Decrement;
//]
//
//
//let getTotalSocialGood (world : WorldState) : float =
//    0.5 // PLACEHOLDER TILL WE HAVE TOTAL SOCIAL GOOD
//    
//
//// SInce opinions on rules is a 19 element array of floats
//let ruleToIndex (rule : Rule) : int =
//    match rule with
//    | Shelter(x) -> 
//        match x with | Random -> 0 | Socialism -> 1 | Meritocracy -> 2 | Oligarchy -> 3
//    | Work(x) ->
//        match x with | Everyone -> 4 | Strongest -> 5 | ByChoice -> 6
//    | Food(x) ->
//        match x with | Communism -> 7 | FoodRule.Socialism -> 8 | FoodRule.Meritocracy -> 9 | FoodRule.Oligarchy -> 10
//    | Voting(x) ->
//        match x with | InstantRunoff -> 11 | Approval -> 12 | Borda -> 13 | Plurality -> 14
//    | Sanction(x) ->
//        match x with | NoFoodAndShelter -> 15 | Exile-> 16 | Increment -> 17 | Decrement -> 18
//
//
//// Why did I implement it like this, this was a huge mistake but I don't have time to make this actually nice...
//let indexToRule (index : int) : Rule =
//    match index with
//    | 0 -> Shelter(Random) | 1 -> Shelter(Socialism) | 2 -> Shelter(Meritocracy) | 3 -> Shelter(Oligarchy)
//    | 4 -> Work(Everyone) | 5 -> Work(Strongest) | 6 -> Work(ByChoice)
//    | 7 -> Food(Communism) | 8 -> Food(FoodRule.Socialism) | 9 -> Food(FoodRule.Meritocracy) | 10 -> Food(FoodRule.Oligarchy)
//    | 11 -> Voting(InstantRunoff) | 12 -> Voting(Approval) | 13 -> Voting(Borda) | 14 -> Voting(Plurality)
//    | 15 -> Sanction(NoFoodAndShelter) | 16 -> Sanction(Exile) | 17 -> Sanction(Increment) | 18 -> Sanction(Decrement)
//    | _ -> failwith "This is greater than the number of possible opinions"
//
//// Placeholders for decision making
//let chairDecision (world : WorldState) (proposals : (Rule option * Agent) list) : (Rule option * Agent) list =
//    let ruleOpinionDifference (newRule : Rule) (world : WorldState) (chair : Agent) : float =
//        // Get the opinion of the new rule
//        let newRuleOpinion = 
//            ruleToIndex newRule
//        // Get the opinion of the old rule
//        let oldRuleOpinion =
//            match newRule with
//            | Shelter(_) -> Shelter(world.CurrentShelterRule) |> ruleToIndex
//            | Food(_) -> Food(world.CurrentFoodRule) |> ruleToIndex
//            | Voting(_) -> Voting(world.CurrentVotingRule) |> ruleToIndex
//            | Work(_) -> Work(world.CurrentWorkRule) |> ruleToIndex
//            | Sanction(_) -> Sanction(world.CurrentMaxPunishment) |> ruleToIndex
//        match chair.DecisionOpinions with
//            | Some(opinions) -> 
//                // Subtract them
//                opinions.RuleOpinion.[oldRuleOpinion] - opinions.RuleOpinion.[newRuleOpinion]
//            | None -> failwithf "Should have opinions."
//    proposals
//    |> List.map (fun proposal ->
//        match world.CurrentChair with
//        | Some(chair) -> 
//            match chair.DecisionOpinions with
//            | Some(opinions) -> 
//                let agentOpinion =
//                    List.find (fun agent -> fst agent = snd proposal) opinions.OtherAgentsOpinion
//                    |> snd // Returns the opinion of the agent from the chairs perspective
//                match proposal |> fst with
//                | Some(rule) ->  
//                    let bt =
//                        1.0 
//                        - agentOpinion * chair.Susceptibility 
//                        - chair.SelfConfidence * ruleOpinionDifference rule world chair
//                    // Set to none (veto, removes from list) if the difference is greater than the threshold
//                    if bt > vetoThreshold
//                    then None, (proposal |> snd)
//                    else proposal
//                | None -> None, (proposal |> snd)
//            | None -> failwithf "Should have opinions."
//        | None -> failwithf "Need there to be a chair."
//    )
//    
//
//let agentNominatesSelf (world : WorldState) (agent : Agent) : bool =
//    let totalSocialGood = getTotalSocialGood world 
//    let ni = 
//        (agent.Egotism + agent.Idealism / totalSocialGood) * agent.SelfConfidence
//    ni > nominationThreshold // Will nominate self if ni > nT
//
//
//let voteOnChairCandidates (agent : Agent) (allCandidates : Agent list) : Agent list =
//    match agent.DecisionOpinions with
//        | Some(opinions) -> opinions.OtherAgentsOpinion
//        | None -> failwithf "Should have opinions of other agents"
//    |> List.sortBy snd // sort list by size of opinion
//    |> List.rev // Get largest to smallest
//    |> List.filter (fun el -> List.contains (el |> fst) allCandidates) // filter by elements only in both lists
//    |> List.map fst // map agent * float to agent    
//
//
//let makeProposals (agent : Agent) (world : WorldState) : (Rule option * Agent) list =
//    // NOTE, KEEP A LIST EVEN IF THEY ONLY CHOOSE ONE PROPOSAL
//    // RULE OPTION -> AGENT CAN EITHER PROPOSE A RULE OR NOT PROPOSE ANYTHING
//    // Agent makes proposal if current rule is their least favourite rule
//    // There is a much better way of doing this if we redefine how stuff works, but I can't be arsed to do that at this point
//    let agentOpinions =
//        match agent.DecisionOpinions with
//        | Some(opinions) -> opinions.RuleOpinion
//        | None -> failwithf "Agent should have opinions."
//    let curShelterMin = 
//        let m = List.min agentOpinions.[0..3]
//        agentOpinions.[Shelter(world.CurrentShelterRule) |> ruleToIndex] <= m, m
//    let curFoodMin =
//        let m = List.min agentOpinions.[4..6]
//        agentOpinions.[Work(world.CurrentWorkRule) |> ruleToIndex] <= m, m
//    let curVoteMin =
//        let m = List.min agentOpinions.[7..10]
//        agentOpinions.[Food(world.CurrentFoodRule) |> ruleToIndex] <= m, m
//    let curSanctionMin =
//        let m = List.min agentOpinions.[11..14]
//        agentOpinions.[Voting(world.CurrentVotingRule) |> ruleToIndex] <= m, m
//    let curWorkMin =
//        let m = List.min agentOpinions.[15..18]
//        agentOpinions.[Sanction(world.CurrentMaxPunishment) |> ruleToIndex] <= m, m
//    let favRules = 
//        let maxIndex (list : 'a list) : int =
//            List.findIndex (fun el -> el = List.max list) list
//        [ // List of rules with (highest opinion float * highest opinion rule)
//            List.max agentOpinions.[0..3], Shelter(allShelterRules.[maxIndex agentOpinions.[0..3]]); // Fav shelter rule
//            List.max agentOpinions.[4..6], Work(allWorkRules.[maxIndex agentOpinions.[4..6] - 4]); // Fav work rule
//            List.max agentOpinions.[7..10], Food(allFoodRules.[maxIndex agentOpinions.[7..10] - 7]); // Fav food rule
//            List.max agentOpinions.[11..14], Voting(allVotingSystems.[maxIndex agentOpinions.[11..14] - 11]); // Fav voting system
//            List.max agentOpinions.[15..18], Sanction(allSanctionVotes.[maxIndex agentOpinions.[15..18] - 15]); // Fav sanction
//        ]
//    let isLeastFavRule = 
//        [curShelterMin; curFoodMin; curVoteMin; curSanctionMin; curWorkMin] // Get list corresponding to whether current rule is least favourite or not
//    let differences = 
//        isLeastFavRule    
//        |> List.mapi (fun i el -> 
//            if el |> fst // = true
//            then Some((favRules.[i] |> fst) - (isLeastFavRule.[i] |> snd), favRules.[i] |> snd) // Float containing the max difference
//            else None)
//    match differences with
//    | [] -> [None, agent] // No new proposal
//    | list -> 
//        match list with 
//        | [h] -> 
//            match h with // One element in list so return that
//            | None -> [None, agent] 
//            | Some(x) -> [Some(x |> snd), agent]
//        | _ -> // more than one element in list
//            let proposal = 
//                list
//                |> List.map (fun el -> 
//                    match el with // Get rid of None values
//                    | Some(x) -> x // Forgot how to map Some(x) to x even if you know there is no None
//                    | None -> (-999.9, Shelter(Random))) // Shouldn't ever happen
//                |> List.filter (fun el -> el <> (-999.9, Shelter(Random)))
//                |> List.maxBy fst // Get one with the biggest difference
//                |> snd
//            [(Some(proposal), agent)]
//
//
//let voteOnProposals (world : WorldState) (agent : Agent)
//    (toVote : ShelterRule list option * WorkAllocation list option * FoodRule list option * VotingSystem list option * Punishment list option)
//    : ShelterRule list option * WorkAllocation list option * FoodRule list option * VotingSystem list option * Punishment list option =
//    // Stuff to vote on will either be a list of values or a None if noone proposed a proposal change
//    // Reorder the lists of stuff to vote on by the agents opinions
//    // This whole thing is horrible but i'm tired and it should work
//    let mapToShelterRule (rule : Rule * float) : ShelterRule * float =
//        match rule with
//        | Shelter(x), f -> x, f
//        | _ -> failwithf "This should be a shelter rule."
//    let mapToWorkRule (rule : Rule * float) : WorkAllocation * float =
//        match rule with
//        | Work(x), f -> x, f
//        | _ -> failwithf "This should be a work rule."
//    let mapToFoodRule (rule : Rule * float) : FoodRule * float =
//        match rule with
//        | Food(x), f -> x, f
//        | _ -> failwithf "This should be a food rule."
//    let mapToVotingSystem (rule : Rule * float) : VotingSystem * float =
//        match rule with
//        | Voting(x), f -> x, f
//        | _ -> failwithf "This should be a voting system."
//    let mapToSanction (rule : Rule * float) : Punishment * float =
//        match rule with
//        | Sanction(x), f -> x, f
//        | _ -> failwithf "This should be a sanction thing."
//    // ^ Can't use discriminated union since they need to be the sepcific rule types, not just ruls
//    let allRules =
//        match agent.DecisionOpinions with
//            | Some(opinions) -> 
//                opinions.RuleOpinion
//                |> List.map (fun el -> (indexToRule (List.findIndex (fun el1 -> el = el1) opinions.RuleOpinion), el))
//            | None -> failwithf "Should have opinions of rules"
//    let shelterRules, workRules, foodRules, votingSystems, sanctions = toVote
//    let shelterVotes = 
//        allRules.[0..3]
//        |> List.sortBy snd
//        |> List.rev // Largest to smallers  
//        |> List.map mapToShelterRule
//        |> fun list -> 
//            match shelterRules with
//            | Some(rules) ->
//                List.filter (fun el -> List.contains (el |> fst) rules) list
//            | None -> []
//        |> function
//            | [] -> None
//            | list -> Some(List.map fst list)
//    let workVotes = 
//        allRules.[4..6]
//        |> List.sortBy snd
//        |> List.rev // Largest to smallers 
//        |> List.map mapToWorkRule 
//        |> fun list -> 
//            match workRules with
//            | Some(rules) ->
//                List.filter (fun el -> List.contains (el |> fst) rules) list
//            | None -> []
//        |> function
//            | [] -> None
//            | list -> Some(List.map fst list)
//    let foodVotes = 
//        allRules.[7..10]
//        |> List.sortBy snd
//        |> List.rev // Largest to smallers  
//        |> List.map mapToFoodRule
//        |> fun list -> 
//            match foodRules with
//            | Some(rules) ->
//                List.filter (fun el -> List.contains (el |> fst) rules) list
//            | None -> []
//        |> function
//            | [] -> None
//            | list -> Some(List.map fst list)
//    let votingVotes = 
//        allRules.[11..14]
//        |> List.sortBy snd
//        |> List.rev // Largest to smallers  
//        |> List.map mapToVotingSystem
//        |> fun list -> 
//            match votingSystems with
//            | Some(rules) ->
//                List.filter (fun el -> List.contains (el |> fst) rules) list
//            | None -> []
//        |> function
//            | [] -> None
//            | list -> Some(List.map fst list)
//    let sanctionVotes = 
//        allRules.[15..18]
//        |> List.sortBy snd
//        |> List.rev // Largest to smallers   
//        |> List.map mapToSanction
//        |> fun list -> 
//            match sanctions with
//            | Some(rules) ->
//                List.filter (fun el -> List.contains (el |> fst) rules) list
//            | None -> []
//        |> function
//            | [] -> None
//            | list -> Some(List.map fst list)        
//    shelterVotes, workVotes, foodVotes, votingVotes, sanctionVotes
//
//
//let getPropositions (world : WorldState) (agents : Agent list)  =
//    // Get all the agents and the proposals they want to make, filter those allowed to make proposals
//    let propositions =
//        agents
//        |> List.collect (fun el -> makeProposals el world)
//        |> chairDecision world // Chair decides which agents can vote
//        |> List.fold (fun acc el ->
//            match el with // Filter out None types so that only the new rule propositions remain
//            | Some(x), l -> acc @ [x, l]
//            | None, _ -> acc
//        ) []
//    // Turn from an (Agent * Rule list) list to an (Rule * Agent list) list (e.g. Proposal list)
//    propositions
//    |> List.fold (fun (acc : Proposal list) el ->
//        let rule = el |> fst
//        if List.contains rule (List.map fst acc)
//        then // This whole function can probably be implemented better...
//            let index = List.findIndex (fun el -> el |> fst = rule) acc
//            List.mapi (fun i el1 ->
//                if i = index
//                then rule, (el1 |> snd) @ [el |> snd]
//                else el1) acc
//        else acc @ [el |> fst, [el |> snd]]
//    ) []
//
//
//let chairVote (world : WorldState) (agents : Agent list) : WorldState =
//    // Vote on the new chair person
//    if world.TimeToNewChair = 0 // Only change chaiman if necessary
//    then // Get the opinions of each agent and carry out a vote on the chairman
//        let candidates = 
//            agents // Does an agent nominate itself
//            |> List.filter (fun agent -> agentNominatesSelf world agent)
//        let newChair =
//            agents
//            |> List.map (fun agent -> voteOnChairCandidates agent candidates)
//            |> match world.CurrentVotingRule with
//               | Borda -> bordaVote
//               | Approval -> approvalVote
//               | InstantRunoff -> instantRunoffVote agents
//               | Plurality -> pluralityVote
//        {world with CurrentChair = Some newChair; TimeToNewChair = 7}
//    else {world with TimeToNewChair = world.TimeToNewChair - 1}
//
//
//let newRules (agents : Agent list) (world : WorldState) (proposals : Proposal list) : ShelterRule option * WorkAllocation option * FoodRule option * VotingSystem option * Punishment option =
//    // Get the rules to vote on
//    let rulesToVoteOn =
//        proposals
//        |> List.fold (fun (acc : ShelterRule list * WorkAllocation list * FoodRule list * VotingSystem list * Punishment list) el ->
//            let acc1, acc2, acc3, acc4, acc5 = acc
//            match el |> fst with
//            | Shelter(x) ->
//                acc1 @ [x], acc2, acc3, acc4, acc5
//            | Work(x) ->
//                acc1, acc2 @ [x], acc3, acc4, acc5
//            | Food(x) ->
//                acc1, acc2, acc3 @ [x], acc4, acc5
//            | Voting(x) ->
//                acc1, acc2, acc3, acc4 @ [x], acc5
//            | Sanction(x) ->
//                acc1, acc2, acc3, acc4, acc5 @ [x]
//        ) ([], [], [], [], []) // Some of these might be empty so need to make option later
//    // Get the agent votes
//    let agentVotes =
//        // Make empty lists into None
//        let optionMake (list : 'a list) : 'a list option =
//            match list with
//            | [] -> None
//            | l -> Some(l)
//        // Since some lists will be None, we need a way of ignoring those lists when they are concatinated
//        let removeNone (list : 'a list option list) : 'a list list option =
//            match list with
//            | x when List.contains None x -> None
//            | x -> List.map (
//                    fun el ->
//                        match el with
//                        | Some(y) -> y
//                        | None -> failwithf "This shouldn't happen since the List should either be all None (already dealt with) or all Some"
//                    ) x |> Some
//        let rulesToVoteOn1 =
//            rulesToVoteOn
//            |> fun (a, b, c, d, e) ->
//                // Get rid of empty lists by setting them to None so decision making has an easier time
//                optionMake a,
//                optionMake b,
//                optionMake c,
//                optionMake d,
//                optionMake e
//        agents
//        |> List.map (fun agent ->
//            rulesToVoteOn1
//            |> voteOnProposals world agent)
//        |> List.fold (fun acc el ->
//            let acc1, acc2, acc3, acc4, acc5 = acc
//            let el1, el2, el3, el4, el5 = el
//            acc1 @ [el1],
//            acc2 @ [el2],
//            acc3 @ [el3],
//            acc4 @ [el4],
//            acc5 @ [el5]
//        ) ([], [], [], [], [])
//        |> fun (a, b, c, d, e) ->
//            // Make a 'list option list' into a 'list list option'
//            removeNone a,
//            removeNone b,
//            removeNone c,
//            removeNone d,
//            removeNone e
//    // Apply current voting system to each of the voting options
//    let votingSystem (allCandidates : 'a list) (candidates : 'a list list) =
//        match world.CurrentVotingRule with
//        | Borda ->
//            bordaVote candidates
//        | Approval ->
//            approvalVote candidates
//        | Plurality ->
//            pluralityVote candidates
//        | InstantRunoff ->
//            instantRunoffVote allCandidates candidates
//    // Vote can result in an option type if there is nothing to vote for, in that case we ignore it
//    let vote (votingSystem) (allCandidates : 'a list) (candidates : 'a list list option) =
//        match candidates with
//        | Some(x) ->
//            votingSystem allCandidates x
//            |> Some
//        | None -> None
//    // Get the ballots
//    let shelterVotes, workVotes, foodVotes, votingVotes, sanctionVotes = agentVotes
//    // Calculate winner based on votes and return as a tuple
//    vote votingSystem allShelterRules shelterVotes,
//    vote votingSystem allWorkRules workVotes,
//    vote votingSystem allFoodRules foodVotes,
//    vote votingSystem allVotingSystems votingVotes,
//    vote votingSystem allSanctionVotes sanctionVotes
//
//
//let implementNewRules (world : WorldState) (rulesToImplement : ShelterRule option * WorkAllocation option * FoodRule option * VotingSystem option * Punishment option) : WorldState =
//    // Implement the new rules
//    let newShelterRule, newWorkRule, newFoodRule, newVotingSystem, newSanction = rulesToImplement
//    // If rule is None due to no vote on it, set it to the old rule
//    let applyOptionRule (newRule : 'a option) (oldRule : 'a) : 'a =
//        match newRule with
//        | Some(x) -> x
//        | None -> oldRule
//    // Get all new stuff except max sanctions
//    let newWorld = {
//        world with
//            CurrentShelterRule = applyOptionRule newShelterRule world.CurrentShelterRule;
//            CurrentWorkRule = applyOptionRule newWorkRule world.CurrentWorkRule;
//            CurrentFoodRule = applyOptionRule newFoodRule world.CurrentFoodRule;
//            CurrentVotingRule = applyOptionRule newVotingSystem world.CurrentVotingRule;
//        }
//    // Punishment can be either incr or decr or changing max punishment
//    match newSanction with
//    | x when x = Some(Increment) ->
//        {newWorld with
//            CurrentSanctionStepSize = newWorld.CurrentSanctionStepSize + 0.1}
//    | x when x = Some(Decrement) ->
//        {newWorld with
//            CurrentSanctionStepSize = newWorld.CurrentSanctionStepSize - 0.1}
//    | _ -> // If not Some(increment) or Some(decrement) then must be a max sanction update or None
//        {newWorld with
//            CurrentMaxPunishment =
//                applyOptionRule newSanction world.CurrentMaxPunishment
//        }
//
//
//let fullDuma (agents : Agent list) (world : WorldState) : WorldState =
//    // Do chair vote
//    let newWorld =
//        agents
//        |> chairVote world
//    // Apply all the duma stuff returning a worldstate containing the new ruleset
//    agents
//    |> getPropositions newWorld
//    |> newRules agents newWorld
//    |> implementNewRules newWorld