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

type Candidate = string
    
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
    AccessToShelter : bool;
    Opinions : (int * float) list
    //Food : float;
    //HunterLevel : float;
    //HunterExp : int;
    //FavouriteFood : Fauna;
    }

type WorldState = {
    VotingType : VotingSystem;
    Buildings : float list
    }


