﻿module Hunt

open Types
open Voting
open Config

// Vote on what to hunt
let voteOnWhatToHunt (votingSystem : VotingSystem) (agents : Agent list) : Fauna =
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
    |> function
        | "Staggi" -> Staggi
        | "Rabbos" -> Rabbos
        | _ -> failwithf "There should be no other possible elements."


// In hunt return list of agents with food and a value that is leftover for the builders to eat
// For each hunter agent, generate a a poisson distributio that contains the probabilities of catching between
// zero and ten animals in the given time fram. Then a random number is generated and the closest probability
// in the list correspponds to the number of animals they agent catches.
//
// Then, based on seflessness, the hunter splits their catch between themself and what will be pooled to feed the
// builders.
//
// After each hunt the hunters gain exp and when they reach a new level the probability of catching animals increases.
let hunt (whatToHunt : Fauna) (huntLength : float) (agents : Agent list) : Agent list * float =
    // Factorial function
    let factorial n =
        let rec loop i acc =
            match i with
            | 0 -> acc
            | 1 -> acc
            | _ -> loop (i - 1) (acc * i)
        loop n 1

    // Poisson distribution:
    let lambda = 
        match whatToHunt with
        | Rabbos -> rabbosProbability * huntLength
        | Staggi -> staggiProbability * huntLength

    // Generate possible animals that are hunted
    let maxListSize = 10
    let probs = 
        List.init maxListSize (fun el -> 
            lambda ** (float el) * (2.71828182845905 ** lambda) / (float (factorial el)))

    agents
    |> List.map (fun agent ->
        let rand = new System.Random()
        let x = (float (rand.Next(0, 100))) / 100.0
        let i = 
            List.tryFindIndex (fun el -> x > el) probs
            |> function
                | None -> maxListSize - 1
                | Some i -> i - 1
        {agent with Food = agent.HuntingAptitude * (1.0 + agent.HunterLevel / 10.0) * probs.[i]})
    |> List.fold ( // Split up the food
        fun acc agent ->
            let agentGiveAway = // Food agent gives away
                agent.Food * agent.Selflessness
            let agentKeep = // Food agent keeps
                agent.Food - agentGiveAway
            // Hunter exp and level updated
            let hunterExp = 
                if agent.HunterExp < 100 
                then agent.HunterExp + expPerHunt 
                else 0
            let level = 
                if agent.HunterExp = 0
                then agent.HunterLevel + 1.0
                else agent.HunterLevel
            // New agent values
            let newAgent = 
                {agent with 
                    Food = agentKeep; 
                    HunterExp = hunterExp; 
                    HunterLevel = level}
            ((acc |> fst) @ [ newAgent ], (acc |> snd) + agentGiveAway)
        ) ([], 0.0)


// Assign the excess food to the agents that didn't hunt
let assignExcessFood (excessFood : float) (nonHunterAgents : Agent list) : Agent list =
    // Get how many agents need food
    let numAgents = 
        nonHunterAgents
        |> List.length
    // Get how much food each agent gets
    let foodSplit = 
        numAgents 
        |> float
        |> (/) excessFood

    nonHunterAgents
    |> List.map (fun agent -> {agent with Food = foodSplit})




// chance of failing a hunt
let failHunt (chanceFail: float): bool = 
    let rand = new System.Random()
    let num = (float (rand.Next(0, 100)))/100.0
    if  num >= (1.0-chanceFail) then true
    else false


// deterimines how many hares are captured
let capHare (energyAllocated: float) (energyRequired: float): float = 
    let numHare = 
        energyAllocated/energyRequired
    floor numHare
    

// determines how many stags are captured based on input list of energy allocated to hunt
let capStag (energyAllocated: float list) (collectiveThreshold: float): float = 
    let totalEnergy = 
        energyAllocated
        |> List.sum
    let numStag =
        totalEnergy/collectiveThreshold
    floor numStag



let agentAction (ego: float)(sucept: float)(idealism: float)(paySocList: float list)(payIndivList: float list): int*float = 
    let equation (paySoc: float)(payIndiv: float)= 
        idealism*paySoc + ego*payIndiv
        |> (/)(idealism*sucept)

    let combine xs ys = 
        List.zip xs ys
        
    combine paySocList payIndivList 
    |> List.map (fun (x,y) -> equation x y)
    |> Seq.mapi (fun i v -> i, v)
    |> Seq.maxBy snd
    //|> (fun i v -> v)


agentAction 5.0 3.0 2.0 [2.0;3.0][4.0;3.0]
|> printfn "%A" 
    


    












                                