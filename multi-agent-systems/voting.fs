module Voting

open Types

// All votes are submitted as a list, even if there is only one value in the list

// Borda Count
let bordaCount (rankings : Candidate list list) : Candidate  =
    // Get rankings for one list
    let getRanking (ranks : Candidate list) : (Candidate * int) list =
        let n = List.length ranks
        ranks
        |> List.mapi (fun i el -> el, n - i) 
        |> List.sort
    let borda = 
        rankings
        |> List.map (fun el -> getRanking el)
    // Sum all the rankings and return the candiate with the highest score
    borda   
    |> List.tail
    |> List.fold (fun acc el1 -> 
        List.mapi (fun i el2 -> fst el2, snd el2 + snd el1.[i]) acc) borda.Head
    |> List.reduce (fun acc el -> if snd el > snd acc then el else acc)
    |> fst

// Plurality voting
let plurality (votes : Candidate list list) : Candidate  =
    votes
    |> List.map (fun el -> el.Head)
    |> List.countBy id
    |> List.maxBy (fun el -> snd el)
    |> fst

// Approval Voting
let approval (votes : Candidate list list) : Candidate  =
    votes
    |> List.concat
    |> List.countBy id
    |> List.maxBy (fun el -> snd el)
    |> fst

// Runoff Voting
let runOffRound1 (votes : Candidate list list) : Candidate * Candidate =
    // Round 1 involves getting the two most popular candidates
    votes
    |> List.map (fun el -> el.Head)
    |> List.countBy id
    |> function
        | [(x, _)] -> x, x
        | [(x, _); (y, _)] -> x, y
        | ((h1, _)::(h2, _)::_) -> h1, h2
        | [] -> failwithf "List shouldn't be empty." // Update this so that if it is an empty list then it still does something
// Runoff Round 2 voting
// This will need to be in a conditional since it only happens if there is no majority in round 1
let runOffRound2 (votes : Candidate list list) : Candidate  =
    votes
    |> plurality // Round 2 is just plurality so reuse that

// Instant Runoff
let instantRunoff (votes : Candidate list list) (allCandidates : Candidate list) : Candidate  =
    // Count the number of first place votes everyone has
    let countVotes (votes : Candidate list list) (remainingCandidates : Candidate list) : (Candidate * int) list =
        let initialAcc = 
            remainingCandidates
            |> List.map (fun el -> el, 0)
        votes
        |> List.filter (fun list -> not (List.isEmpty list))
        |> List.fold (fun acc rank -> 
            let firstPlace = List.head rank
            let index = List.findIndex (fun el -> fst el = firstPlace) acc
            List.mapi (fun i el -> if i = index then fst el, (snd el) + 1 else el) acc
            ) initialAcc 
    // Get the candidate with the least number of first place votes
    let roundLoser (count : (Candidate * int) list) : Candidate =
        count
        |> List.minBy (fun el -> snd el)
        |> fst
    // Return the list of candidates without the round loser   
    let newRanks (allVotes : Candidate list list) (roundLoser : Candidate) : Candidate list list =
        allVotes
        |> List.map (List.filter (fun el -> not (el = roundLoser)) )
    // Recursively run the rounds
    let rec runoff (votes : Candidate list list) (remainingCandidates : Candidate list ) : Candidate =
        printf "\nir %A" votes 
        printf "\ncv %A" (countVotes votes remainingCandidates)
        match countVotes votes remainingCandidates with
        | [(x, _)] -> x
        | list -> 
            let loser = roundLoser list
            let nextRoundRanks = newRanks votes loser
            let remaining = List.filter (fun el -> not (el = loser)) remainingCandidates
            printf "\nr %A" remaining 
            runoff nextRoundRanks remaining
  
    runoff votes allCandidates


