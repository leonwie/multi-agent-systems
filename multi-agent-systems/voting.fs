module Voting

open Agent
open Types

type Candidate = string

// Borda Count
let bordaCount (rankings : Candidate list list) : (Candidate * int) list =
    // Get rankings for one list
    let getRanking (ranks : Candidate list) : (Candidate * int) list =
        let n = List.length ranks
        ranks
        |> List.mapi (fun i el -> el, n - i) 
        |> List.sort
    let borda = 
        rankings
        |> List.map (fun el -> getRanking el)
    
    // Sum all the rankings
    borda   
    |> List.tail
    |> List.fold (fun acc el1 -> List.mapi (fun i el2 -> fst el2, snd el2 + snd el1.[i]) acc) borda.Head

        
