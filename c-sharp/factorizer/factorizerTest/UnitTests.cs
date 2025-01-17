using static factorizer.MathClasses;
using static factorizer.MathLatex;
using static factorizer.UtilityFunctions;
// ReSharper disable UnusedType.Global

namespace factorizerTest;

public class UnitTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestMathTermToLatex()
    {
        MathVariable[] mathNumbers =
        [
            new MathVariable
            {
                Name = 'y'
            },
            new MathVariable
            {
                Name = 'x',
                Exponent = 3
            }
        ];

        MathTerm mathTerm = new MathTerm(mathNumbers)
        {
            Coefficient = 5
        };

        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
    
        Assert.Equal("+5yx^{3}", mathTerm.StringRepresentation);
    }
    
    [Fact]
    public void TestMathTermStringRepresentation()
    {
        MathVariable[] mathNumbers =
        [
            new MathVariable
            {
                
                Name = 'y'
            },
            new MathVariable
            {
                Name = 'x',
                Exponent = 3
            }
        ];

        MathTerm mathTerm = new MathTerm(mathNumbers)
        {
            Coefficient = 5
        };
    
        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
        mathTerm.GetVariablesByName('x')[0].Exponent = 4;
        mathTerm.GetVariablesByName('y')[0].Exponent = 69;
        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
        
        Assert.Equal("+5y^{69}x^{4}", mathTerm.StringRepresentation);
    }

    [Fact]
    public void TestLatexToMathTerm()
    {
        MathVariable[] mathNumbers = // x^{2}6x9y
        [ 
            new MathVariable {
                Name = 'x',
                Exponent = 2
            },
            new MathVariable {
                Name = 'x'
            },
            new MathVariable {
                Name = 'y'
            }
        ];
        
        MathTerm mathTerm = new MathTerm(mathNumbers)
        {
            Coefficient = 54
        };
        MathTerm mathTerm2 = LatexTermToMathTerm("x^{2}6x9y");
        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
        // testOutputHelper.WriteLine(mathTerm2.StringRepresentation);
        Assert.Equal(mathTerm.StringRepresentation, mathTerm2.StringRepresentation);
    }
    
    [Fact]
    public void TestLatexToMathExpression()
    {
        MathVariable[] mathNumbers1 = // -57x^{2}xy
        [ 
            new MathVariable {
                Name = 'x',
                Exponent = 2
            },
            new MathVariable {
                Name = 'x'
            },
            new MathVariable {
                Name = 'y'
            }
        ];
        
        MathVariable[] mathNumbers2 = // 54xy
        [
            new MathVariable {
                Name = 'x'
            },
            new MathVariable {
                Name = 'y'
            }
        ];
        
        MathExpression mathExpression1 = new MathExpression(
            new MathTerm(mathNumbers1) { Coefficient = -57 },
            new MathTerm(mathNumbers2) { Coefficient = 54 }
            );
        MathExpression mathExpression2 = LatexExpressionToMathExpression("-57x^{2}xy+54xy");
        // PrintMathExpression(mathExpression1);
        // PrintMathExpression(mathExpression2);
        
        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
        // testOutputHelper.WriteLine(mathTerm2.StringRepresentation);
        Assert.Equal(mathExpression1.StringRepresentation, mathExpression2.StringRepresentation);
    }

    [Fact]
    public void TestCombineMathExpressionMathTerms()
    {
        MathExpression mathExpression1 = LatexExpressionToMathExpression("-47x^{3}y+5y^{4}");
        MathExpression mathExpression2 = LatexExpressionToMathExpression("-57x^{2}xy+5x^{2}xy2+2y^{4}+3y^{4}");
        mathExpression2 = CombineMathExpressionMathTerms(mathExpression2);
        
        Assert.Equal(mathExpression1.StringRepresentation, mathExpression2.StringRepresentation);
    }
}