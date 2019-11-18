module Agent

open Types
open Config

// Initialise Agent
let initialiseAgent (id : int) (numAgents : int) : Agent =
    {
        Name = "Agent " + (id |> string);
        Selflessness = ((System.Random().Next(minSelfless, maxSelfless)) |> float) / 100.0; 
        BuildingAptitude =(System.Random().Next(minAptitude, maxAptitude)) |> float;
        HuntingAptitude = (System.Random().Next(minAptitude, maxAptitude)) |> float;
        PoliticalApathy = (System.Random().Next(minPoliticalApathy, maxPoliticalApathy)) |> float;
        FavouriteFood = // For when it comes to hunting staggi or robbos
            match (System.Random().Next(0, 100)) |> float with
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

        
let agents = 
    List.init numAgents (fun el -> initialiseAgent el numAgents)