using factorizer.Models;
using static factorizer.Latex.LatexToMath;
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
                Exponent = 3,
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
        // Assert.Equal("+5y^{3}x^{3}", mathTerm.StringRepresentation);
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
    public void TestLatexToMathParentheses()
    {
        // \\left(69x^{2}-6\\right)\\left(6x+9\\right)\\left(9y+7\\right)
        
        MathExpression mathExpression1 = new MathExpression(
            new MathTerm([
                new MathVariable {
                    Name = 'x',
                    Exponent = 2
                }
            ]) { Coefficient = 69 },
            new MathTerm { Coefficient = -6 }
        );
        
        MathExpression mathExpression2 = new MathExpression(
            new MathTerm([
                new MathVariable {
                    Name = 'x'
                }
            ]) { Coefficient = 6 },
            new MathTerm { Coefficient = 9 }
        );
        MathExpression mathExpression3 = new MathExpression(
            new MathTerm([
                new MathVariable {
                    Name = 'y'
                }
            ]) { Coefficient = 9 },
            new MathTerm { Coefficient = 7 }
        );

        MathParentheses mathParenthesis1 = new MathParentheses([
            mathExpression1,
            mathExpression2,
            mathExpression3
        ]);
        
        MathParentheses mathParenthesis2 = 
            LatexParenthesesToMathParentheses("\\left(69x^{2}-6\\right)\\left(6x+9\\right)\\left(9y+7\\right)");
        // PrintMathExpression(mathExpression1);
        // PrintMathExpression(mathExpression2);
        
        testOutputHelper.WriteLine(mathParenthesis1.StringRepresentation);
        testOutputHelper.WriteLine(mathParenthesis2.StringRepresentation);
        Assert.Equal(mathParenthesis1.StringRepresentation, mathParenthesis2.StringRepresentation);
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