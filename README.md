# multi-agent-systems
This is the repo for the Self Organising Multi-Agent Systems Assessed Project

# 0. Build and compile:

Developed with __dotnetcore 3.0__ (might be compatible w other versions)

* To build (from the GitHub cloned directory do) :

      dotnet build .\multi-agent-systems.sln
* To run (from the GitHub cloned directory do) :

      cd  .\multi-agent-systems\bin\Debug\netcoreapp3.0\
      .\multi-agent-systems.exe  --number-days -1 --number-profiles 7 --number-agents 24
      
# 1. Run the program w the default configuration:

   In order to run from the command line Baseline Configuration (12 balanced agents, 2 from each other category) use the following command line arguments:

     --number-days -1 --number-profiles 7 --number-agents 24

  where:
  * --number-days x -> x is the number of days the simulation runs for, if -1 runs until all agents are dead 
  * --number-profiles x -> x is the number of agent profiles there are (number of profie_ files in the dir multi-agent-systems\Agent-Config\agent_dir)
  * --number-agents x -> x is the number of agents that take part in the simulation initially - this has to be consistent w the number given in the python script
  
# 2. Run the program w your own configuration:

* Modify in multi-agent-systems\Agent-Config\agent_init.py TotalProfiles:

      TotalProfiles = [(Balanced, 12), (Egotist, 2), (Idealist, 2), (Susceptible, 2),
                       (NotIdealist, 2), (NotEgotist, 2), (NotSusceptible, 2)]                 
   in order to choose how many agents of which types you'd like to have    

* Run multi-agent-systems\Agent-Config\agent_init.py in order to generate the profiles

* Run the f sharp executable with the command line args specified previously (don't forget to change the number-days, number-profiles, number-agents accordingly)

# 3. Modify other program variables:

In Config.fs there are other config variables that can be changed for testing & experimenting in dev stages. They are not meant to be modified during the runtime when a user runs the simulation.
