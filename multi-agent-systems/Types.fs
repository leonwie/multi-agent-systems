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
    AccessToShelter : float option;
    Food : float;
    HunterLevel : float;
    HunterExp : int;
    Opinions : (string * float) list    // Perhaps change string to int and add an ID field
    }



type Shelter = {
    Quality : float
}

type Rule = string // PlaceHolder
type ImmutableRule = string // PlaceHolder

type WorldState = {
    VotingType : VotingSystem;
    Buildings : float list
    Policies : (Rule * bool) list;
    System : ImmutableRule list;
    CurrentTurn : int;
    NumStag: int;
    NumHare: int;
    }