using static factorizer.MathClasses;
using static factorizer.MathLatex;

namespace factorizer;

public class Tests
{
    public static void TestAllTests()
    {
        // Console.WriteLine("TestMathTermToLatex:");
        // TestMathTermToLatex();
        // Console.WriteLine("\n\nTestMathTermStringRepresentation:");
        // TestMathTermStringRepresentation();
        Console.WriteLine("\n\nTestLatexToMathTerm:");
        TestLatexToMathTerm();
    }
    
    public static void TestMathTermToLatex()
    {
        MathNumber testing = new MathNumber
        {
            Coefficient = 5,
            Name = 'y'
        };
        // Console.WriteLine("\n\n\nspecialtest\n\n\n");
        // UtilityFunctions.PrintMathVariable(testing);
        // UtilityFunctions.PrintMathNumber(testing);
        // UtilityFunctions.PrintMathVariable(testing);
        // Console.WriteLine("\n\n");
        
        
        
        MathNumber[] mathNumbers =
        [
            new MathNumber
            {
                Coefficient = 5,
                Name = 'y'
            },
            new MathNumber
            {
                Name = 'x',
                Exponent = 3
            }
        ];

        MathTerm mathTerm = new MathTerm(mathNumbers);
    
        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
        
        Console.WriteLine($"+5yx^{{3}} = {mathTerm.StringRepresentation}");
        // Assert.Equal("5yx^{3}", mathTerm.StringRepresentation);
    }
    
    public static void TestMathTermStringRepresentation()
    {
        MathNumber[] mathnumbers =
        [
            new MathNumber
            {
                
                Name = 'y',
                Coefficient = 5
            },
            new MathNumber
            {
                Name = 'x',
                Exponent = 3
            }
        ];

        MathTerm mathTerm = new MathTerm(mathnumbers);
    
        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
        mathTerm.GetVariablesByName('x')[0].Exponent = 4;
        mathTerm.GetVariablesByName('y')[0].Exponent = 69;
        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
        
        Console.WriteLine($"5y^{{69}}x^{{4}} = {mathTerm.StringRepresentation}");
        // Assert.Equal("5y^{69}x^{4}", mathTerm.StringRepresentation);
    }

    public static void TestLatexToMathTerm()
    {
        MathNumber[] mathNumbers = // x^{2}6x9
        [ 
            new MathNumber {
                Name = 'x',
                Exponent = 2
            },
            new MathNumber {
                Name = 'x',
                Coefficient = 6
            },
            new MathNumber {
                Coefficient = 9
            }
        ];
        
        MathTerm mathTerm = new MathTerm(variables: mathNumbers);
        MathTerm mathTerm2 = LatexTermToMathTerm("x^{2}6x9");
        Console.WriteLine(mathTerm.StringRepresentation);
        Console.WriteLine(mathTerm2.StringRepresentation);
        
        Console.WriteLine($"{mathTerm2.StringRepresentation} = {mathTerm.StringRepresentation}");
        // Assert.Equal(mathTerm2.StringRepresentation, mathTerm.StringRepresentation);
    }
}