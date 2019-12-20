module Agent

open Types

// Generate random numbers
let rand = System.Random()

// Initialise Agent
let initialiseAgent (id : int) (susceptibility : float) (egotism : float) (idealism : float): Agent =
    {
        ID = id;                                // Unique ID
        Susceptibility = susceptibility;        // in [0, 1]
        Egotism = egotism;                      // in [0, 1]
        Idealism = idealism;                    // in [0, 1]
       
        Reward = 0.0;
        Friends = List.empty<Agent>;
        Enemies = List.empty<Agent>;
        Infamy = 0.0;
        Energy = 100.0;
        TodaysActivity = Activity.NONE, 0.0;
        AccessToShelter = Some 1.0;
        BuildingAptitude = 0.0;
        HuntingAptitude = 0.0;
    }

