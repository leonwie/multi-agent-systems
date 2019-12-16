module WorldState

open Types

// Not much done here yet

let currentWorld = {
    VotingType = Approval;
    Buildings = [];
    CurrentChair = None;
    TimeToNewChair = 7;
    CurrentShelterRule = Random;
    CurrentVotingRule = Approval;
    CurrentFoodRule = Communism;
    CurrentWorkRule = Everyone;
    }