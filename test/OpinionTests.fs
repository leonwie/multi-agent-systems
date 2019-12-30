module Tests
open NUnit.Framework
open Types
open Opinion
open Agent

[<TestFixture>]
type TestClass () =
    // Generate a random number between [leftBound, rightBound]
    let generateRandom leftBound rightBound =
        let seed = System.Random()
        if rightBound < leftBound then
            seed.NextDouble() + leftBound
        else    
            seed.NextDouble() * (rightBound - leftBound) + leftBound
            
    [<DefaultValue>] val mutable agents : Agent list
    [<DefaultValue>] val mutable state : WorldState

    [<SetUp>]
    member this.SetUp () =
        setNumberAgents(24)
        this.agents <- initialiseBalancedAgents 24
        this.state <- currentWorld
        
    let currentWorld =
        {
            Buildings = List.Empty;
            TimeToNewChair = 5;
            CurrentShelterRule = Random;
            CurrentFoodRule = Communism;
            CurrentVotingRule = Borda;
            CurrentWorkRule = Everyone;
            CurrentMaxPunishment = Exile;
            CurrentSanctionStepSize = 0.1;
            CurrentDay = 0;
            CurrentChair = None;
            NumHare = 15;
            NumStag = 15;
            CurrentRuleSet = initialiseRuleSet;
            AllRules = initialiseAllRules;
            BuildingRewardPerDay = 0.0;
            HuntingRewardPerDay = 0.0;
            BuildingAverageTotalReward = 0.0;
            HuntingAverageTotalReward = 0.0;
        }

    let idealistAgent id =
        let idealism = generateRandom 0.5 0.9
        let susceptibility = generateRandom 0.15 1.0 - idealism - 0.15
        let egotism = 1.0 - idealism - susceptibility
        initialiseAgent id susceptibility egotism idealism 
        
    let egotistAgent id =
        let egotism = generateRandom 0.5 0.9
        let susceptibility = generateRandom 0.05 1.0 - egotism - 0.05
        let idealism = 1.0 - egotism - susceptibility
        initialiseAgent id susceptibility egotism idealism
   
    let susceptibleAgent id =
        let susceptibility = generateRandom 0.5 0.9
        let egotism = generateRandom 0.05 1.0 - susceptibility - 0.05
        let idealism = 1.0 - egotism - susceptibility
        initialiseAgent id susceptibility egotism idealism

    let balancedAgent id =
        let susceptibility = generateRandom 0.25 0.38
        let egotism = generateRandom 0.25 0.38
        let idealism = 1.0 - egotism - susceptibility
        initialiseAgent id susceptibility egotism idealism        
    
    let notIdealistAgent id =
        let egotism = generateRandom 0.3 0.7
        let susceptibility = 1.0 - egotism
        let idealism = 0.0
        initialiseAgent id susceptibility egotism idealism
        
    let notEgotistAgent id =
        let idealism = generateRandom 0.3 0.7
        let susceptibility = 1.0 - idealism
        let egotism = 0.0
        initialiseAgent id susceptibility egotism idealism
        
    let notSusceptibleAgent id =
        let idealism = generateRandom 0.3 0.7
        let egotism = 1.0 - idealism
        let susceptibility = 0.0
        initialiseAgent id susceptibility egotism idealism
    
    // Initial try with 24 agents - baseline config
    let initialiseBalancedAgents total =
        let balanced = List.map balancedAgent [(total / 2)..(total - 1)]
        let halfMargin = total / 2
        let fs = [idealistAgent; susceptibleAgent; egotistAgent; notSusceptibleAgent; notEgotistAgent; notIdealistAgent]
        let elemPerType = halfMargin / fs.Length
        let intervalList = List.map (fun f -> [(elemPerType * (f - 1))..(elemPerType * f - 1)]) [1 .. halfMargin / elemPerType]
        let zipped = List.zip fs intervalList
        let rest = List.collect (fun (f, interval) -> List.map f interval) zipped
        initialiseAgentDecisions (balanced @ rest)
        
    [<Test>]
    member this.updateRewardsForEveryRuleForAgentTest() =
        let before = List.map (fun agent -> agent.DecisionOpinions.Value.RewardPerRule) this.agents
        let updatedAgents = updateRewardsForEveryRuleForAgent this.state this.agents 
        let after = List.map (fun agent -> agent.DecisionOpinions.Value.RewardPerRule) updatedAgents
        printf "BEFORE%A\n" before
        printf "AFTER%A" after
        Assert.True(not (before = after))
        
    [<Test>]
    member this.updateSocialGoodForEveryCurrentRuleTest() =
        let updatedWorld = updateSocialGoodForEveryCurrentRule this.agents this.state
        printf "BEFORE%A\n" this.state.CurrentRuleSet
        printf "AFTER%A" updatedWorld.CurrentRuleSet
        Assert.True(not (this.state.CurrentRuleSet = updatedWorld.CurrentRuleSet))
    
    [<Test>]
    member this.normaliseTests() =
        let gainAgents = List.map (fun agent -> {agent with Gain = 4}) this.agents
        let updatedAgents = updateRewardsForEveryRuleForAgent this.state gainAgents 
        let updatedWorld = updateSocialGoodForEveryCurrentRule updatedAgents this.state
        let newAgents = normaliseTheAgentArrays updatedAgents
        let newState = normaliseTheSocialGood updatedWorld
        Assert.True(not(newAgents = gainAgents))
        printf "BEFORE%A\n" this.state.CurrentRuleSet
        printf "AFTER%A" newState.CurrentRuleSet
        Assert.True(not(newState.CurrentRuleSet = this.state.CurrentRuleSet))

    [<Test>]
    member this.updateAggregationArrayForAgentTest() =
        let before = List.map (fun agent -> agent.DecisionOpinions.Value.OverallRuleOpinion) this.agents
        let updatedAgents = updateAggregationArrayForAgent this.state this.agents 
        let after = List.map (fun agent -> agent.DecisionOpinions.Value.OverallRuleOpinion) updatedAgents
        printf "BEFORE%A\n" before.[0]
        printf "AFTER%A" after.[0]
        Assert.True(not (before = after))
