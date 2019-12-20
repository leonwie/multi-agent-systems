module Agent

open Types
open Config

// Generate random numbers
let rand = System.Random()

// Initialise Agent
let initialiseAgent (profile : string) (id : int)  (susceptibility : float)  (idealism : float)  (egotism : float) (explorationProbability : float)
                     (alternativeChoiceProbability : float) (reward: float) (friends : Agent list) (enemies : Agent list) (infamy: float) (energy : float) 
                     (todaysActivity : Activity * float) (accessToShelter : float option) (opinions : (int * float) list) (buildingAptitude: float)
                     (huntingAptitude : float) (mood : int) (selflessness : float) : Agent =
    {
        Profile = profile;                      // Agent profile type
        ID = id;                                // Unique ID
        Susceptibility = susceptibility;        // in [0, 1]
        Idealism = idealism;        // in [0, 1]
        Egotism = egotism;        // in [0, 1]
        ExplorationProbability = explorationProbability;    // in [0, 1]
        AlternativeChoiceProbability = alternativeChoiceProbability;
        Reward = reward;
        Friends = friends;
        Enemies = enemies;
        Infamy = infamy;
        Energy = energy;
        TodaysActivity = todaysActivity;
        AccessToShelter = accessToShelter;
        Opinions = opinions;
        BuildingAptitude = buildingAptitude;
        HuntingAptitude = huntingAptitude;
        Mood = mood;
        Selflessness = selflessness;
    }

// Initialise a list of agents
//let agents = 
//    List.init numAgents (fun el -> initialiseAgent el numAgents)


