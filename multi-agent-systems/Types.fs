﻿module Types

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

type SocialGood = float
type LastUpdate = int
type RuleSet = (Rule * SocialGood * LastUpdate) list
let initialiseRuleSet = [(Shelter(ShelterRule.Random), 0.5, 0); (Food(FoodRule.Communism), 0.5, 0); (Work(Everyone), 0.5, 0);
                         (Voting(Borda), 0.5, 0); (Sanction(Exile), 0.5, 0)]
let initialiseAllRules = [(Shelter(ShelterRule.Random), 0.5, 0); (Shelter(ShelterRule.Oligarchy), 0.5, 0); 
                          (Shelter(ShelterRule.Meritocracy), 0.5, 0); (Shelter(ShelterRule.Socialism), 0.5, 0);
                          (Food(FoodRule.Communism), 0.5, 0); (Food(FoodRule.Oligarchy), 0.5, 0);
                          (Food(FoodRule.Socialism), 0.5, 0); (Food(FoodRule.Meritocracy), 0.5, 0);
                          (Work(Everyone), 0.5, 0); (Work(ByChoice), 0.5, 0);
                          (Work(Strongest), 0.5, 0); (Voting(Borda), 0.5, 0);
                          (Voting(Approval), 0.5, 0); (Voting(InstantRunoff), 0.5, 0);
                          (Voting(Plurality), 0.5, 0); (Sanction(Exile), 0.5, 0);
                          (Sanction(NoFoodAndShelter), 0.5, 0); (Sanction(Increment), 0.5, 0);
                          (Sanction(Decrement), 0.5, 0);]
let ShelterRuleList = [Shelter(ShelterRule.Random); Shelter(ShelterRule.Oligarchy); Shelter(ShelterRule.Meritocracy); Shelter(ShelterRule.Socialism)]
let FoodRuleList = [Food(FoodRule.Communism); Food(FoodRule.Meritocracy); Food(FoodRule.Oligarchy); Food(FoodRule.Socialism)]
let PunishmentList = [Sanction(NoFoodAndShelter); Sanction(Increment); Sanction(Decrement); Sanction(Exile)]
let VotingSystemList = [Voting(Approval); Voting(InstantRunoff); Voting(Borda); Voting(Plurality)]
let WorkAllocationList = [Work(Everyone); Work(ByChoice); Work(Strongest)]

// All agents in Opinions are shallow copies, ONLY REFER THEM BY ID, do NOT use any other fields from the copies
type Opinions =
    {
        RewardPerRule : (Rule * float * LastUpdate) list;        // reward per rule - has all rules
        PersonalCurrentRulesOpinion : (Rule * float) list;       // this is X from the spec; changes after each day/time slice = X(t)
        InitialRuleOpinion : (Rule * float) list;                // does not change after initialisation = X(0)
        OverallRuleOpinion : (Rule * float) list;                // this is O from the spec
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

        Gain : float;
        EnergyDeprecation : float;
        EnergyConsumed : float;
        Infamy : float;
        Energy : float;
        HuntedFood : float;
        DecisionOpinions : Opinions option;

        TodaysActivity : Activity * float;
        AccessToShelter : float option;
        SelfConfidence : float;
        TodaysHuntOption : int;

        R : (float * LastUpdate) list;
        RhuntingEnergySplit : (float * LastUpdate) list;
        Rsharing : (float * LastUpdate) list;
        FoodShared : bool;

        LastCrimeDate : int;
        AccessToFood : bool;
        Alive: bool;
    }

// All agents in WorldState are shallow copies, ONLY REFER THEM BY ID, do NOT use any other fields from the copies    
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
        BuildingRewardPerDay : float;
        HuntingRewardPerDay : float;
        BuildingAverageTotalReward : float;
        HuntingAverageTotalReward : float;

        S : float list;     // Social good per work
        // Social good of each of the 11 options of hunting energy split types
        // 1st entry: full energy to stag -> last entry: full energy to hare
        ShuntingEnergySplit : float list;   
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


