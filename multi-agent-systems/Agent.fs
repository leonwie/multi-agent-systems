module Agent

open Types
open Config

// Generate random numbers
let rand = System.Random()

// Initialise Agent
let initialiseAgent (profile : string) (id : int)  (selflessness : float)  (buildingAptitude : float)  (huntingAptitude : float) (political : float)
                     (mood : int) (energy : float) (todaysActivity : Activity * float) (accessToShelter : float option) (opinions : (int * float) list)  : Agent =
    {
        Profile = profile;
        ID = id;
        Selflessness = selflessness;
        BuildingAptitude = buildingAptitude;
        HuntingAptitude = huntingAptitude;
        PoliticalApathy = political;
        Mood = mood;
        Energy = energy;
        TodaysActivity = todaysActivity;
        AccessToShelter = accessToShelter;
        Opinions = opinions;
    }

// Initialise a list of agents
//let agents = 
//    List.init numAgents (fun el -> initialiseAgent el numAgents)