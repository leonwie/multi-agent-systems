module WorldState

open Types
open Agent

let currentWorld = {
    Buildings = [];
    CurrentChair = None;
    TimeToNewChair = 7;
    CurrentShelterRule = Random;
    CurrentVotingRule = Approval;
    CurrentFoodRule = Communism;
    CurrentWorkRule = Everyone;
    CurrentMaxPunishment = NoFoodAndShelter;
    CurrentSanctionStepSize = 0.1;
    //Policies = List.empty<Rule * bool>;
    //System = List.empty<ImmutableRule>;
    CurrentDay = 1;
    NumStag = 10;
    NumHare = 10;
    }

let energyProfile (agents : Agent list) =
    agents
    |> List.map (fun el -> el.ID,el.Energy)

