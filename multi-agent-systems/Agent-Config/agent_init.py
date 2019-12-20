import json
import os
import shutil
from agent import *

TotalProfiles = [(Agent, 3), (Egotist, 5)]
TotalNumAgent = sum([i[1] for i in TotalProfiles])

# Reset agent definition directory
if os.path.exists("agent_dir"):
    shutil.rmtree("agent_dir")
os.mkdir("agent_dir")

# Generate agent specification json based on TotalProfiles
counter = 0
for profileIndex in range(len(TotalProfiles)):
    agent = TotalProfiles[profileIndex][0](counter, counter + TotalProfiles[profileIndex][1], TotalNumAgent)
    counter += TotalProfiles[profileIndex][1]
    # Agent customisation from here
    with open(f"agent_dir/profile{profileIndex}.json", "w") as profile:
        json.dump(agent.__dict__, profile, indent=4)
