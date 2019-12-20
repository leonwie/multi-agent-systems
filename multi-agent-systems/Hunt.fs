module Hunt

open Types
open Voting
open Config

//// Vote on what to hunt
//let voteOnWhatToHunt (votingSystem : VotingSystem) (agents : Agent list) : Fauna =
//    let getFaunaRanking (agent : Agent) : string list =
//        match agent.FavouriteFood with
//        | Staggi -> ["Staggi"; "Rabbos"] // If more animals added then set FavouriteFood as Head and randomise Tail order
//        | Rabbos -> ["Rabbos"; "Staggi"]       
    
//    agents
//    |> List.map getFaunaRanking
//    |> match votingSystem with
//        | Borda -> bordaVote
//        | Approval -> approvalVote
//        | InstantRunoff -> instantRunoffVote ["Staggi"; "Rabbos"]
//        | Plurality -> pluralityVote 
//    |> function
//        | "Staggi" -> Staggi
//        | "Rabbos" -> Rabbos
//        | _ -> failwithf "There should be no other possible elements."


//// In hunt return list of agents with food and a value that is leftover for the builders to eat
//// For each hunter agent, generate a a poisson distributio that contains the probabilities of catching between
//// zero and ten animals in the given time fram. Then a random number is generated and the closest probability
//// in the list correspponds to the number of animals they agent catches.
////
//// Then, based on seflessness, the hunter splits their catch between themself and what will be pooled to feed the
//// builders.
////
//// After each hunt the hunters gain exp and when they reach a new level the probability of catching animals increases.
//let hunt (whatToHunt : Fauna) (huntLength : float) (agents : Agent list) : Agent list * float =
//    // Factorial function
//    let factorial n =
//        let rec loop i acc =
//            match i with
//            | 0 -> acc
//            | 1 -> acc
//            | _ -> loop (i - 1) (acc * i)
//        loop n 1

//    // Poisson distribution:
//    let lambda = 
//        match whatToHunt with
//        | Rabbos -> rabbosProbability * huntLength
//        | Staggi -> staggiProbability * huntLength

//    // Generate possible animals that are hunted
//    let maxListSize = 10
//    let probs = 
//        List.init maxListSize (fun el -> 
//            lambda ** (float el) * (2.71828182845905 ** lambda) / (float (factorial el)))

//    agents
//    |> List.map (fun agent ->
//        let rand = new System.Random()
//        let x = (float (rand.Next(0, 100))) / 100.0
//        let i = 
//            List.tryFindIndex (fun el -> x > el) probs
//            |> function
//                | None -> maxListSize - 1
//                | Some i -> i - 1
//        {agent with Food = agent.HuntingAptitude * (1.0 + agent.HunterLevel / 10.0) * probs.[i]})
//    |> List.fold ( // Split up the food
//        fun acc agent ->
//            let agentGiveAway = // Food agent gives away
//                agent.Food * agent.Selflessness
//            let agentKeep = // Food agent keeps
//                agent.Food - agentGiveAway
//            // Hunter exp and level updated
//            let hunterExp = 
//                if agent.HunterExp < 100 
//                then agent.HunterExp + expPerHunt 
//                else 0
//            let level = 
//                if agent.HunterExp = 0
//                then agent.HunterLevel + 1.0
//                else agent.HunterLevel
//            // New agent values
//            let newAgent = 
//                {agent with 
//                    Food = agentKeep; 
//                    HunterExp = hunterExp; 
//                    HunterLevel = level}
//            ((acc |> fst) @ [ newAgent ], (acc |> snd) + agentGiveAway)
//        ) ([], 0.0)


//// Assign the excess food to the agents that didn't hunt
//let assignExcessFood (excessFood : float) (nonHunterAgents : Agent list) : Agent list =
//    // Get how many agents need food
//    let numAgents = 
//        nonHunterAgents
//        |> List.length
//    // Get how much food each agent gets
//    let foodSplit = 
//        numAgents 
//        |> float
//        |> (/) excessFood

//    nonHunterAgents
//    |> List.map (fun agent -> {agent with Food = foodSplit})




// chance of failing a hunt
let failHunt (chanceFail: float): bool = 
    let rand = new System.Random()
    let num = (float (rand.Next(0, 100)))/100.0
    if  num >= (1.0-chanceFail) then true
    else false


// deterimines how many hares are captured
let capHare (energyRequired: float) (chanceFail: float) (energyAllocated: float): float = 
    let numHare = 
        energyAllocated
        |> (/) energyRequired
        |> floor
        |> int
    
    // Set faile hunts to 0 and find sum
    seq {for _ in 1 .. numHare -> 1}
    |> Seq.map (fun _ -> if failHunt chanceFail then 0.0 else 1.0)
    |> Seq.sum


// Check if stag hunt meets criteria for success
let meetStagCondition (actProfile: float list) (weakLink: float) (energyToCapture: float): bool = 
    let minEnergy actProfile weakLink =
        if List.min actProfile >= weakLink then true 
        else false
    
    
    let thresholdEnergy actProfile energyToCapture =
        if List.sum actProfile >= energyToCapture then true 
        else false
            
    
    if (minEnergy actProfile weakLink && thresholdEnergy actProfile energyToCapture) then true
    else false


// determines how many stags are captured based on input list of energy allocated to hunt
let capStag (weakLink: float) (collectiveThreshold: float) (chanceFail: float)  (energyAllocated: float list) : float = 
    
    let maxNumStag =
        energyAllocated
        |> List.sum
        |> (/) collectiveThreshold
        |> floor
        |> int

    let numStag = 
        seq {for _ in 1 .. maxNumStag -> 1}
        |> Seq.map (fun _ -> if failHunt chanceFail then 0.0 else 1.0)
        |> Seq.sum

    match meetStagCondition energyAllocated weakLink collectiveThreshold with
    | false -> 0.0
    | true -> numStag


  
    
let regenRate (rate: float) (totNum: int) (maxCapacity: int) : int =
    totNum
    |> float
    |> (/) (maxCapacity |> float)
    |> fun x -> 1.0 - x
    |> (*) rate
    |> (*) (totNum |> float)
    |> ceil
    |> int

