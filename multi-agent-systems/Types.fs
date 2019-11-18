module Types

type Activity =
    | Building
    | Hunting
    | Nothing

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
    Name : string;
    Selflessness : float; 
    BuildingAptitude : float;
    HuntingAptitude : float;
    PoliticalApathy : float;
    FavouriteFood : Fauna;
    Mood : int;
    Energy : float;
    TodaysActivity : Activity * float;
    AccessToShelter : bool;
    Food : float;
    HunterLevel : float;
    HunterExp : int;
    Opinions : (string * float) list    // Perhaps change string to int and add an ID field
    }

type WorldState = {
    VotingType : VotingSystem;
    Buildings : float list
    }


