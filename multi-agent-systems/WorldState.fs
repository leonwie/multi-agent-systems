module WorldState

open Types
open Agent1

let mutable currentWorld = {
    VotingType = Borda;
    Buildings = [];
    Policies = List.empty<Rule * bool>;
    System = List.empty<ImmutableRule>;
    CurrentTurn = 1;
    }

let energyProfile (agents : Agent1 list) =
    agents
    |> List.map (fun el -> el.ID * el.Energy)