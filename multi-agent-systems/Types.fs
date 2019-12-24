module Types

type Activity =
    | NONE
    | HUNTING
    | BUILDING

type Fauna =
    | Rabbos
    | Staggi

type VotingSystem =
    | InstantRunoff
    | Approval
    | Borda
    | Plurality

type FoodRule =
    | Communism // Equal Split
    | Socialism // Weakest get more
    | Meritocracy // Biggest contributors get more
    | Oligarchy // Strongest get more

type ShelterRule =
    | Random
    | Socialism // Weakest get more
    | Meritocracy // Biggest contributors get more
    | Oligarchy // Strongest get more

type WorkAllocation =
    | Everyone
    | Strongest
    | ByChoice

type Punishment =
    | NoFoodAndShelter
    | Exile
    | Increment
    | Decrement

type Rule =
    | Shelter of ShelterRule
    | Food of FoodRule
    | Voting of VotingSystem
    | Work of WorkAllocation
    | Sanction of Punishment

// I assummed in some places that the rules have an index
// e.g. CurrentRulesOpinion - the first element in list is for SHELTER, second FOOD etc
type RuleTypes =
    | SHELTER = 1
    | FOOD = 2
    | WORK = 3
    | VOTING = 4
    | SANCTION = 5

type SocialGood = float
type LastUpdate = int
type RuleSet = (RuleTypes * Rule * SocialGood * LastUpdate) list
let initialiseRuleSet = [(RuleTypes.SHELTER, Shelter(Random), 0.5, 0); (RuleTypes.FOOD, Food(Communism), 0.5, 0);
                             (RuleTypes.WORK, Work(Everyone), 0.5, 0); (RuleTypes.VOTING, Voting(Borda), 0.5, 0);
                             (RuleTypes.SANCTION, Sanction(Exile), 0.5, 0)]
let initialiseAllRules = [(RuleTypes.SHELTER, Shelter(Random), 0.5, 0); (RuleTypes.SHELTER, Shelter(Oligarchy), 0.5, 0);
                          (RuleTypes.SHELTER, Shelter(Meritocracy), 0.5, 0); (RuleTypes.SHELTER, Shelter(Socialism), 0.5, 0);
                          (RuleTypes.FOOD, Food(Communism), 0.5, 0); (RuleTypes.FOOD, Food(FoodRule.Oligarchy), 0.5, 0);
                          (RuleTypes.FOOD, Food(FoodRule.Socialism), 0.5, 0); (RuleTypes.FOOD, Food(FoodRule.Meritocracy), 0.5, 0);
                          (RuleTypes.WORK, Work(Everyone), 0.5, 0); (RuleTypes.WORK, Work(ByChoice), 0.5, 0);
                          (RuleTypes.WORK, Work(Strongest), 0.5, 0); (RuleTypes.VOTING, Voting(Borda), 0.5, 0);
                          (RuleTypes.VOTING, Voting(Approval), 0.5, 0); (RuleTypes.VOTING, Voting(InstantRunoff), 0.5, 0);
                          (RuleTypes.VOTING, Voting(Plurality), 0.5, 0); (RuleTypes.SANCTION, Sanction(Exile), 0.5, 0);
                          (RuleTypes.SANCTION, Sanction(NoFoodAndShelter), 0.5, 0); (RuleTypes.SANCTION, Sanction(Increment), 0.5, 0);
                          (RuleTypes.SANCTION, Sanction(Decrement), 0.5, 0);]
type Opinions =
    {
        RewardPerRule : (Rule * float * LastUpdate) list;         // reward per rule - has all rules
        PersonalCurrentRulesOpinion : float list;                // this is X from the spec; changes after each day/time slice = X(t)
        InitialRuleOpinion : float list;                         // does not change after initialisation = X(0)
        OverallCurrentRuleOpinion : float list;                  // this is O from the spec (current)
        PastRulesOpinion : (RuleTypes * Rule * float) list;      // this is O from the spec (past) changes every time an opinion changes - put here the old ones
        AllOtherAgentsOpinion : (Agent * float) list;            // this is A from the spec warning: Agent here is a shallow copy - has DecisionOpinions : None
        Friends : Agent list;                                    // warning: Agent here is a shallow copy - has DecisionOpinions : None
        Enemies : Agent list;                                    // warning: Agent here is a shallow copy - has DecisionOpinions : None
    }
and Agent =
    {
        ID : int;
        Susceptibility : float;
        Idealism : float;
        Egotism : float;

        Gain : int;
        EnergyDeprecation : float;
        EnergyConsumed : float;
        Infamy : float;
        Energy : float;
        DecisionOpinions : Opinions option;

        TodaysActivity : Activity * float;
        AccessToShelter : float option;
        BuildingAptitude : float;
        HuntingAptitude : float;
        SelfConfidence : float;
        
        R : float list;
        Rsharing : float list;
        S : float list;
    }

type WorldState =
    {
        Buildings : float list;
        CurrentChair : Agent option;
        TimeToNewChair : int;
        CurrentShelterRule : ShelterRule;
        CurrentVotingRule : VotingSystem;
        CurrentFoodRule : FoodRule;
        CurrentWorkRule : WorkAllocation;
        CurrentMaxPunishment : Punishment;
        CurrentSanctionStepSize : float;
        CurrentDay : int;
        NumHare : int;
        NumStag : int;
        CurrentRuleSet : RuleSet;
        AllRules : RuleSet;
    }

type WorldProperties =
    {
        Tau : float;
        Gamma : float;
    }
type Shelter =
   {
        Quality : float
   }

type Proposal = Rule * Agent list


