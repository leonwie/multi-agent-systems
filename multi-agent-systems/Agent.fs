module Agent
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
       
        EnergyDeprecation = 0.0;                // end-of-turn energy loss
        Gain = 0.0;                               // energy received for the day
        EnergyConsumed = 0.0;                   // energy spent on work
        Infamy = 0.0;
        Energy = 100.0;
        HuntedFood = 0.0;                       // Food from hunting (non-zero for hunters only)
        
        TodaysActivity = Activity.NONE, 0.0;
        TodaysHuntOption = 0;                   // Hunting energy split option, valid for hunters only
        AccessToShelter = None;
        DecisionOpinions = None;
        SelfConfidence = 0.5;

        // 11 options of hunting energy split types
        // 1st entry: full energy to stag -> last entry: full energy to hare
        RhuntingEnergySplit = List.init 11 (fun _ -> 0.5);
        R = [0.5; 0.5; 0.5];    // NONE, HUNT, BUILDING

        Rsharing = [0.5; 0.5];

        LastCrimeDate = 0;
        AccessToFood = true;
        Alive = true;
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
     if (agents.Length <> numAgents) then
         failwith "The number of agents specified in the command line is different from the
                number of agents specified in the profile files. Please make it consistent!"
     let allAgentOpinions = List.map (fun _ -> List.zip agents (generateRandom numAgents)) agents
     let opinions = List.map createOpinions allAgentOpinions
     let updatedOpinions = List.zip (List.map id agents) opinions
     updatedOpinions |>
     List.map (fun (agent, newOpinion) -> {agent with DecisionOpinions = Some newOpinion})

// Public function for post agent creation - initialises decisions
let initialiseAgentDecisions (agents : Agent list) : Agent list =
    let opinions = initialiseDecisionOpinions agents
    List.map (initialiseFriendsAndEnemies opinions) opinions

