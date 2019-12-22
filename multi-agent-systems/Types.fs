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

type Reward = float
type SocialGood = float

type RuleSet = (Rule * Reward * SocialGood) list

type Opinions =
    {
        RewardPerRule : Reward list;                          // changes after each day/time slice = X(t)
        RuleOpinion : float list;                             // changes after each day/time slice = X(t)
        InitialRuleOpinion : float list;                      // does not change after initialisation = X(0)
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


