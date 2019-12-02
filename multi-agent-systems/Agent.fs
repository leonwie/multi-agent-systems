module Agent

open Types
open Config

// Generate random numbers
let rand = System.Random()

// Initialise Agent
let initialiseAgent (id : int) (numAgents : int) : Agent =
    {
        Name = "Agent " + (id |> string);
        Selflessness = ((rand.Next(minSelfless, maxSelfless)) |> float) / 100.0; 
        BuildingAptitude =(rand.Next(minAptitude, maxAptitude)) |> float;
        HuntingAptitude = (rand.Next(minAptitude, maxAptitude)) |> float;
        PoliticalApathy = (rand.Next(minPoliticalApathy, maxPoliticalApathy)) |> float;
        FavouriteFood = // For when it comes to hunting staggi or robbos
            match (rand.Next(0, 100)) |> float with
                | x when x < 50.0 -> Staggi
                | _ -> Rabbos
        Mood = 100;
        Energy = 100.0;
        TodaysActivity = Nothing, 0.0;
        AccessToShelter = false;
        Food = 0.0;
        HunterLevel = 0.0;
        HunterExp = 0;
        Opinions = List.init numAgents (fun el -> "Agent " + (el |> string), 50.0) // Default opinions are 50 and can increase or decrease
    }

// Initialise a list of agents
let agents = 
    List.init numAgents (fun el -> initialiseAgent el numAgents)