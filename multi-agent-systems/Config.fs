module Config

// This is the config, its the place where you canhnge the values for stuff


let mutable maxSimulationTurn = -1 // Set in Parsing as a cmd arg; Negative value corresponds to infinity
let mutable numAgents = 0
let numberOfRules = 5

let staggiEnergyValue = 200.0
let staggiProbability = 0.1 // likelihood is 1 in 10 intervals
let staggiMeanRegenRate = 0.1
let staggiMinIndividual = 10.0
let staggiMinCollective = 50.0
let rabbosEnergyValue = 50.0
let rabbosProbability = 0.3 // likelihood is 3 in 10 intervals
let rabbosMinRequirement = 10.0
let rabbosMeanRegenRate = 0.1



let foodSaturation = 40


let costOfHunting = 30

let eb = 10.0 // energy cost per worker to build
let em = 2.5 // energy cost per worker to maintain
let rg = 0.05 // shelter quality decay rate
let es = 35.0 // energy cost per worker to build one shelter
let ep = 0.8 // maximum shelter energy preservation
let rb = 5.0 //base energy decay rate

let maxNumStag = 30
let maxNumHare = 30

let vetoThreshold = 2.0
let nominationThreshold = 0.5

// New Spec
let ExplorationDecay = 0.5


// For Sanctions
let InfamyStep = 0.1
let MinimumFoodForOligarchy = 0.5
let CrimeDiscoveryRate = 0.5
let WorkExemptionThreshold = 0.3