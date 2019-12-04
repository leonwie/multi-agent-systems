module Config

// This is the config, its the place where you canhnge the values for stuff

let numAgents = 16

let minSelfless = 20
let maxSelfless = 60

let minAptitude = 30
let maxAptitude = 80

let minPoliticalApathy = 10
let maxPoliticalApathy = 50

let costOfBuilding = 50
let costOfHunting = 30

let shelterEnergySavings = 0.5
// let shelterDecayRate = 25.0 // Shelter lasts 4 days

let staggiEnergyValue = 200
let staggiProbability = 0.1 // likelihood is 1 in 10 intervals
let rabbosEnergyValue = 50
let rabbosProbability = 0.3 // likelihood is 3 in 10 intervals

let foodSaturation = 40

let huntingTime = 8.0

let expPerHunt = 15



let eb = 0.0 // energy cost per worker to build
let em = 0.0 // energy cost per worker to maintain
let rg = 0.0 // shelter quality decay rate
let es = 0.0 // energy cost per worker to build one shelter
let ep = 0.0 // maximum shelter energy preservation
let rb = 0.0 //base energy decay rate

let maxSimulationTurn = infinity