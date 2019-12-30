module Agent
open System
open System
open Config
open Types

// Generate random numbers
let rand = System.Random()

// Generate count random numbers in [0,1]
let generateRandom count =
    List.init count (fun _ -> rand.NextDouble())

// Look up friendToFind in the Friends & Enemies lists of agent
let private lookUp (agents : Agent list) (agent : Agent) (friendToFind : Agent) : bool * bool =
   let index = List.tryFindIndex (fun e -> e = agent) agents
   match index with
   | None -> false, false
   | Some index ->
       match agents.[index].DecisionOpinions with
       | None -> false, false
       | Some decision -> List.exists (fun elem -> elem = friendToFind) decision.Friends,
                          List.exists (fun elem -> elem = friendToFind) decision.Enemies
 
// Public function to initialise Agent
let initialiseAgent (id : int) (susceptibility : float) (egotism : float) (idealism : float) : Agent =
    {
        ID = id;                                // Unique ID
        Susceptibility = susceptibility;        // in [0, 1]
        Egotism = egotism;                      // in [0, 1]
        Idealism = idealism;                    // in [0, 1]
       
        EnergyDeprecation = 0.0;
        Gain = 0;
        EnergyConsumed = 0.0;
        Infamy = 0.0;
        Energy = 100.0;
        
        TodaysActivity = Activity.NONE, 0.0;
        AccessToShelter = None;
        BuildingAptitude = 0.0;
        HuntingAptitude = 0.0;
        DecisionOpinions = None;
        SelfConfidence = 0.5;

        R = [0.5; 0.5; 0.5; 0.5];    // NONE, STAG, HARE, BUILDING
        S = [0.5; 0.5; 0.5; 0.5];
        Rsharing = [0.5; 0.5];

        LastCrimeDate = 0;
        AccessToFood = true;
        Alive = true;
        TodaysEnergyObtained = 0.0;
        TodaysFoodCaptured = 0.0;
    }

let private initialRewardRule = List.map (fun (y, _, _) -> (y, 0.5, 0)) initialiseAllRules

let private initialOverallRuleOpinion () =
    List.map (fun (y, _, _) ->
    (y,((generateRandom 1) |> List.head) * (0.6 - 0.4) + 0.4)) initialiseAllRules

// Private function to create the opinion type with only one way friends/enemies
let private createOpinions (opinions : (Agent * float) list) : Opinions =
    {
        InitialRuleOpinion = initialOverallRuleOpinion ()
        RewardPerRule = initialRewardRule
        PersonalCurrentRulesOpinion = initialOverallRuleOpinion ()
        OverallRuleOpinion = initialOverallRuleOpinion ()
        AllOtherAgentsOpinion = opinions
        Friends = List.filter (fun (_, op) -> op > 0.5) opinions |> List.map fst
        Enemies = List.filter (fun (_, op) -> op < 0.1) opinions |> List.map fst
    }

// Private function to create both way firend/enemy mapping
let private initialiseFriendsAndEnemies (agents : Agent list) (agent : Agent)  : Agent =
    if
        agent.DecisionOpinions.IsNone then agent
    else
        let filteredFriends = List.filter (fun ag -> lookUp agents ag agent |> fst) agent.DecisionOpinions.Value.Friends
        let filteredEnemies = List.filter (fun ag -> lookUp agents ag agent |> snd) agent.DecisionOpinions.Value.Enemies
        let updatedDecision = {agent.DecisionOpinions.Value with Friends = filteredFriends; Enemies = filteredEnemies}
        {agent with DecisionOpinions = Some updatedDecision}

// Private function to create and return the DecisionOptions type
let private initialiseDecisionOpinions (agents : Agent list) : Agent list =
     let allAgentOpinions = List.map (fun _ -> List.zip agents (generateRandom numAgents)) agents
     let opinions = List.map createOpinions allAgentOpinions
     let updatedOpinions = List.zip (List.map id agents) opinions
     updatedOpinions |>
     List.map (fun (agent, newOpinion) -> {agent with DecisionOpinions = Some newOpinion})

let setNumberAgents (numberAgents : int) =
    numAgents <- numberAgents
    
// Public function for post agent creation - initialises decisions
let initialiseAgentDecisions (agents : Agent list) : Agent list =
    setNumberAgents(agents.Length)
    let opinions = initialiseDecisionOpinions agents
    List.map (initialiseFriendsAndEnemies opinions) opinions
