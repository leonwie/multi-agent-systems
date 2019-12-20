from enum import Enum

class Agent:
    '''
    Inherit this as a base class to develop parameteter initialisation for each agent profile
    '''
    def __init__(self, start, end, totalNumAgent):
        self.Profile = self.__class__.__name__
        self.IDRange = list(range(start, end))
        self.Susceptibility = 0.5
        self.Idealism = 0.5
        self.Egotism = 0.5

class DefaultAgent(Agent):
    def __init__(self, start, end, totalNumAgent):
        super().__init__(start, end, totalNumAgent)

class Egotist(Agent):
    def __init__(self, start, end, totalNumAgent):
        super().__init__(start, end, totalNumAgent)
        self.Egotism = 0.8
        self.Susceptibility = 0.1
        self.Idealism = 0.1
