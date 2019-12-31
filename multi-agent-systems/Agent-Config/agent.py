from enum import Enum
import random

class Agent:
    '''
    Inherit this as a base class to develop parameteter initialisation for each agent profile
    This is the same as Balanced
    '''
    def __init__(self, start, end, totalNumAgent):
        self.Profile = self.__class__.__name__
        self.IDRange = list(range(start, end))
        self.Egotism = [random.uniform(0.25, 0.38) for _ in range(end - start + 1)]
        self.Susceptibility = [random.uniform(0.25, 0.38) for _ in range(end - start + 1)]
        self.Idealism = [1 - self.Susceptibility[i] - self.Egotism[i] for i in range(end - start + 1)]

class Idealist(Agent):
    def __init__(self, start, end, totalNumAgent):
        super().__init__(start, end, totalNumAgent)
        self.Idealism = [random.uniform(0.5, 0.9) for _ in range(end - start + 1)]
        self.Susceptibility = [random.uniform(0.15, 1 - self.Idealism[i] - 0.15) for i in range(end - start + 1)]
        self.Egotism = [1 - self.Susceptibility[i] - self.Idealism[i] for i in range(end - start + 1)]

class Egotist(Agent):
    def __init__(self, start, end, totalNumAgent):
        super().__init__(start, end, totalNumAgent)
        self.Egotism = [random.uniform(0.5, 0.9) for _ in range(end - start + 1)]
        self.Susceptibility = [random.uniform(0.05, 1 - self.Egotism[i] - 0.05) for i in range(end - start + 1)]
        self.Idealism = [1 - self.Susceptibility[i] - self.Egotism[i] for i in range(end - start + 1)]

class Susceptible(Agent):
    def __init__(self, start, end, totalNumAgent):
        super().__init__(start, end, totalNumAgent)
        self.Susceptibility = [random.uniform(0.5, 0.9) for _ in range(end - start + 1)]
        self.Egotism = [random.uniform(0.05, 1 - self.Susceptibility[i] - 0.05) for i in range(end - start + 1)]
        self.Idealism = [1 - self.Susceptibility[i] - self.Egotism[i] for i in range(end - start + 1)]

class Balanced(Agent):
    def __init__(self, start, end, totalNumAgent):
        super().__init__(start, end, totalNumAgent)
        self.Egotism = [random.uniform(0.25, 0.38) for _ in range(end - start + 1)]
        self.Susceptibility = [random.uniform(0.25, 0.38) for _ in range(end - start + 1)]
        self.Idealism = [1 - self.Susceptibility[i] - self.Egotism[i] for i in range(end - start + 1)]

class NotIdealist(Agent):
    def __init__(self, start, end, totalNumAgent):
        super().__init__(start, end, totalNumAgent)
        self.Egotism = [random.uniform(0.3, 0.7) for _ in range(end - start + 1)]
        self.Susceptibility = [1 - self.Egotism[i] for i in range(end - start + 1)]
        self.Idealism = [0.0] * (end - start + 1)

class NotEgotist(Agent):
    def __init__(self, start, end, totalNumAgent):
        super().__init__(start, end, totalNumAgent)
        self.Idealism = [random.uniform(0.3, 0.7) for _ in range(end - start + 1)]
        self.Susceptibility = [1 - self.Idealism[i] for i in range(end - start + 1)]
        self.Egotism = [0.0] * (end - start + 1)

class NotSusceptible(Agent):
    def __init__(self, start, end, totalNumAgent):
        super().__init__(start, end, totalNumAgent)
        self.Idealism = [random.uniform(0.3, 0.7) for _ in range(end - start + 1)]
        self.Egotism = [1 - self.Idealism[i] for i in range(end - start + 1)]
        self.Susceptibility = [0.0] * (end - start + 1)
