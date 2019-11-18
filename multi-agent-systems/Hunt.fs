module Hunt

open Types
open Voting
open Config

// Vote on what to hunt
let voteOnWhatToHunt (votingSystem : VotingSystem) (agents : Agent list) : string =
    let getFaunaRanking (agent : Agent) : string list =
        match agent.FavouriteFood with
        | Staggi -> ["Staggi"; "Rabbos"] // If more animals added then set FavouriteFood as Head and randomise Tail order
        | Rabbos -> ["Rabbos"; "Staggi"]       
    
    agents
    |> List.map getFaunaRanking
    |> match votingSystem with
        | Borda -> bordaVote
        | Approval -> approvalVote
        | InstantRunoff -> instantRunoffVote ["Staggi"; "Rabbos"]
        | Plurality -> pluralityVote 


// In hunt return list of agents with food and a value that is leftover for the builders to eat
let hunt (whatToHunt : Fauna) (huntLength : float) (agents : Agent list) : Agent list * float =
    // Factorial function
    let factorial n =
        let rec loop i acc =
            match i with
            | 0 -> acc
            | 1 -> acc
            | _ -> loop (i-1) (acc * i)
        loop n 1
    // Poisson distribution:
    let lambda = 
        match whatToHunt with
        | Rabbos -> rabbosProbability * huntLength
        | Staggi -> staggiProbability * huntLength

    let probsInOneInterval = 
        List.init 10 (fun el -> lambda ** (float el) * (2.71828182845905 ** lambda) / (float (factorial el)))

    failwithf "Not yet impemented."
    