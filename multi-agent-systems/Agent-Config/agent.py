from enum import Enum

class Activity(str, Enum):
    NONE: str = "NONE"
    HUNT: str = "HUNTING"
    BUILD: str = "BUILDING"

class Agent:
    '''
    Inherit this as a base class to develop parameteter initialisation for each agent profile
    '''
    def __init__(self, start, end, totalNumAgent):
        self.Profile = self.__class__.__name__                      # Profile name is the same as the class name
        self.IDRange = list(range(start, end))                      # ID range assigned to agent of a particular subclass [start, end)
        self.Selflessness = 50.0                                      # [0, 100]
        self.HuntingAptitude = 50.0                                   # [0, 100]
        self.Political = 50.0                                        # [0, 100]
        self.Mood = 50                                              # [0, 100]
        self.Energy = 50.0                                            # [0, 100]
        self.ActivityType = Activity.NONE                             # SEE ENUM CLASS ABOVE
        self.ActivityNumber = 50.0                                    #  [0, 100]
        self.AccessToShelter = 50.0                                    # [0, 100]
        self.BuildingAptitude = 50.0                                  # [0, 100]
        self.Opinions = [50.0] * totalNumAgent                        # ARRAY OF OPINIONS FOR EACH OTHER AGENT


class DefaultAgent(Agent):
    def __init__(self, start, end, totalNumAgent):
        super().__init__(start, end, totalNumAgent)

class HuntingAgent(Agent):
    def __init__(self, start, end, totalNumAgent):
        super().__init__(start, end, totalNumAgent)
        self.Activity = Activity.HUNT
        self.HuntingAptitude = 70.0
