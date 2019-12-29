module DecisionTests
open NUnit.Framework
open Decision

[<TestFixture>]
type TestClass () =
    
    [<Test>]    
    member this.standardizeTest() =
        let newList = standardize [0.1; 0.2; 0.3; 0.4; 0.9; 0.95; 0.99; 0.88; 0.97]
        let standardized = [0.121222544; 0.1923915858; 0.2635606277; 0.3347296695; 0.6905748787;
                            0.7261593996; 0.7546270164; 0.6763410703; 0.740393208]
        let zipped = List.zip newList standardized
        List.map (fun (actual, expected) -> Assert.AreEqual(System.Math.Round((float)actual, 4), System.Math.Round((float)expected, 4))) zipped
        Assert.True(true)
    
    [<Test>]    
    member this.standardizeSameValTest() =
        let newList = standardize [4.0; 4.0; 4.0; 4.0; 4.0]
        printf "stand %A" newList
