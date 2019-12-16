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

type Rule =
    | Shelter of ShelterRule
    | Food of FoodRule
    | Voting of VotingSystem
    | Work of WorkAllocation
    
type Agent = {
    Profile : string;
    ID : int;
    Selflessness : float; 
    BuildingAptitude : float;
    HuntingAptitude : float;
    PoliticalApathy : float;
    Mood : int;
    Energy : float;
    TodaysActivity : Activity * float;
    AccessToShelter : float option;
    Opinions : (int * float) list
    //Food : float;
    //HunterLevel : float;
    //HunterExp : int;
    //FavouriteFood : Fauna;
    }

type WorldState = {
    VotingType : VotingSystem;
    Buildings : float list;
    CurrentChair : Agent option;
    TimeToNewChair : int;
    CurrentShelterRule : ShelterRule;
    CurrentVotingRule : VotingSystem;
    CurrentFoodRule : FoodRule;
    CurrentWorkRule : WorkAllocation;
    }

type Shelter = {
    Quality : float
}

type Proposal = Rule * Agent list
