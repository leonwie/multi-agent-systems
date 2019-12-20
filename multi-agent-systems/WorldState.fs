module WorldState

open Types
open Agent1

let currentWorld = {
<<<<<<< HEAD
<<<<<<< HEAD
    VotingType = Borda;
    Buildings = [];
    Policies = List.empty<Rule * bool>;
    System = List.empty<ImmutableRule>;
    CurrentTurn = 1;
    NumStag = 10;
    NumHare = 10;
    }

let energyProfile (agents : Agent1 list) =
    agents
    |> List.map (fun el -> el.ID * el.Energy)
=======
=======
>>>>>>> master
    VotingType = Approval;
    Buildings = [];
    CurrentChair = None;
    TimeToNewChair = 7;
    CurrentShelterRule = Random;
    CurrentVotingRule = Approval;
    CurrentFoodRule = Communism;
    CurrentWorkRule = Everyone;
<<<<<<< HEAD
    CurrentMaxPunishment = NoFoodAndShelter;
    CurrentSactionStepSize = 0.1;
    }
=======
<<<<<<< HEAD
    }
>>>>>>> master
=======
    }
>>>>>>> master
>>>>>>> game-loop
