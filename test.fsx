let wr = new System.IO.StreamWriter("""C:\Users\Alex\Desktop\test.csv""")
let headings = "Buildings,
                Current Chair, 
                Time to new Chair, 
                Current Shelter Rule, 
                Current Voting Rule,
                Current Food Rule,
                Current Work Rule,
                Current Max Punishment,
                Current Sanction Step Size,
                Current Day,
                Num Hare,
                Num Stag,
                Current Rule Set,
                All Rules, 
                Building Reward Per Day,
                Hunting Reward Per Day,
                Building Average Total Reward,
                Hunting Average Total Reward,
                S,
                Shunting Energy Split,"

let agentHeadings = "[ID]Susceptibility,
                     [ID]Idealism,
                     [ID]Egotism,
                     [ID]Gain,
                     [ID]Energy Depreciation,
                     [ID]Energy Consumed,
                     [ID]Infamy,
                     [ID]Energy,
                     [ID]Hunted Food,
                     [ID]Today's Activity,
                     [ID]Access To Shelter,
                     [ID]Self Confidence,
                     [ID]Today's Hunt Option,
                     [ID]R,
                     [ID]Rhunting Energy Split,
                     [ID]Rsharing,
                     [ID]Food Sharing,
                     [ID]Last Crime Date,
                     [ID]Access To Food,
                     [ID]Alive,"

let agentList = [1; 2; 3]
printfn "test %s" (headings.Replace("\n","").Replace(" ",""))
wr.Write(List.fold (fun acc elem -> acc + agentHeadings.Replace("[ID]",string elem).Replace("\n","").Replace(" ","")) "" agentList)
wr.Write("\n")
wr.Write([1.0;2.0;3.0])
wr.Close()