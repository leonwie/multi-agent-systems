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

type Reward = float
type SocialGood = float

type RuleSet = (Rule * Reward * SocialGood) list

type Opinions =
    {
        CurrentRewardPerRule : Reward list;                   // changes after each day/time slice = X(t)
        CurrentRulesOpinion : float list;                     // changes after each day/time slice = X(t)
        InitialRuleOpinion : float list;                      // does not change after initialisation = X(0)
        PastRulesOpinion : (RuleTypes * Rule * float) list;   // changes every time an opinion changes - put here the old ones
        OtherAgentsOpinion : (Agent * float) list;            // warning: Agent here is a shallow copy - has DecisionOpinions : None
        Friends : Agent list;                                 // warning: Agent here is a shallow copy - has DecisionOpinions : None
        Enemies : Agent list;                                 // warning: Agent here is a shallow copy - has DecisionOpinions : None
    }
and Agent =
    {
        ID : int;
        Susceptibility : float;
        Idealism : float;
        Egotism : float;

        Reward : float;
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
        RuleSet : RuleSet;
        GlobalSocialGood : float;
        AverageSocialGood : float;
    }

type Shelter =
   {
        Quality : float
   }

type Proposal = Rule * Agent list


