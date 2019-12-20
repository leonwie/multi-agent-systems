module Voting

open Types

// All votes are submitted as a list, even if there is only one value in the list

// Borda Count
let bordaVote (rankings : 'a list list) : 'a  =
    // Get rankings for one list
    let getRanking (ranks : 'a list) : ('a * int) list =
        let n = List.length ranks
        ranks
        |> List.mapi (fun i el -> el, n - i) 
        |> List.sort

    let borda = 
        rankings
        |> List.map getRanking

    // Sum all the rankings and return the candiate with the highest score
    borda   
    |> List.reduce (fun acc el1 -> 
        List.mapi (fun i el2 -> fst el2, snd el2 + snd el1.[i]) acc)
    |> List.reduce (fun acc el -> if snd el > snd acc then el else acc)
    |> fst


// Plurality voting
let pluralityVote (votes : 'a list list) : 'a  =
    votes
    |> List.map List.head
    |> List.countBy id
    |> List.maxBy snd
    |> fst


// Approval Voting
let approvalVote (votes : 'a list list) : 'a  =
    votes
    |> List.concat
    |> List.countBy id
    |> List.maxBy snd
    |> fst


// Runoff Voting
let runOffRound1 (votes : 'a list list) : 'a * 'a =
    // Round 1 involves getting the two most popular 'as
    votes
    |> List.map List.head
    |> List.countBy id
    |> function
        | [(x, _)] -> x, x
        | [(x, _); (y, _)] -> x, y
        | ((h1, _)::(h2, _)::_) -> h1, h2
        | [] -> failwithf "List shouldn't be empty." // Update this so that if it is an empty list then it still does something
// Runoff Round 2 voting
// This will need to be in a conditional since it only happens if there is no majority in round 1
let runOffRound2 (votes : 'a list list) : 'a  =
    votes
    |> pluralityVote // Round 2 is just plurality so reuse that


// Instant Runoff
let instantRunoffVote (allCandidates : 'a list) (votes : 'a list list) : 'a  =
    // Count the number of first place votes everyone has
    let countVotes (votes : 'a list list) (remaining'as : 'a list) : ('a * int) list =
        let initialAcc = 
            remaining'as
            |> List.map (fun el -> el, 0)
        votes
        |> List.filter (fun list -> not (List.isEmpty list))
        |> List.fold (fun acc rank -> 
            let firstPlace = List.head rank
            let index = List.findIndex (fun el -> fst el = firstPlace) acc
            List.mapi (fun i el -> if i = index then fst el, (snd el) + 1 else el) acc
            ) initialAcc 

    // Get the 'a with the least number of first place votes
    let roundLoser (count : ('a * int) list) : 'a =
        count
        |> List.minBy snd
        |> fst

    // Return the list of 'as without the round loser   
    let newRanks (allVotes : 'a list list) (roundLoser : 'a) : 'a list list =
        allVotes
        |> List.map (List.filter (fun el -> el <> roundLoser))

    // Recursively run the rounds
    let rec runoff (votes : 'a list list) (remaining'as : 'a list ) : 'a =
        match countVotes votes remaining'as with
        | [(x, _)] -> x
        | list -> 
            let loser = roundLoser list
            let nextRoundRanks = newRanks votes loser
            let remaining = List.filter (fun el -> el <> loser) remaining'as
            runoff nextRoundRanks remaining

    runoff votes allCandidates

