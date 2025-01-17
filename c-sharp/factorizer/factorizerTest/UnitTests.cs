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
    
        Assert.Equal("+5yx^{3}", mathTerm.StringRepresentation);
    }
    
    [Fact]
    public void TestMathTermStringRepresentation()
    {
        MathNumber[] mathNumbers =
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

        MathTerm mathTerm = new MathTerm(mathNumbers);
    
        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
        mathTerm.GetVariablesByName('x')[0].Exponent = 4;
        mathTerm.GetVariablesByName('y')[0].Exponent = 69;
        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
        
        Assert.Equal("+5y^{69}x^{4}", mathTerm.StringRepresentation);
    }

    [Fact]
    public void TestLatexToMathTerm()
    {
        MathNumber[] mathNumbers = // x^{2}6x9y
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
            },
            new MathNumber {
                Name = 'y'
            }
        ];
        
        MathTerm mathTerm = new MathTerm(variables: mathNumbers);
        MathTerm mathTerm2 = LatexTermToMathTerm("x^{2}6x9y");
        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
        // testOutputHelper.WriteLine(mathTerm2.StringRepresentation);
        Assert.Equal(mathTerm2.StringRepresentation, mathTerm.StringRepresentation);
    }
    
    [Fact]
    public void TestLatexToMathExpression()
    {
        MathNumber[] mathNumbers1 = // -x^{2}6x9y
        [ 
            new MathNumber {
                Coefficient = -1,
                Name = 'x',
                Exponent = 2
            },
            new MathNumber {
                Name = 'x',
                Coefficient = 6
            },
            new MathNumber {
                Coefficient = 9
            },
            new MathNumber {
                Name = 'y'
            }
        ];
        
        MathNumber[] mathNumbers2 = // 6x9y
        [
            new MathNumber {
                Name = 'x',
                Coefficient = 6
            },
            new MathNumber {
                Coefficient = 9
            },
            new MathNumber {
                Name = 'y'
            }
        ];
        
        MathExpression mathExpression1 = new MathExpression(
            new MathTerm(mathNumbers1),
            new MathTerm(mathNumbers2)
            );
        MathExpression mathExpression2 = LatexExpressionToMathExpression("-x^{2}6x9y+6x9y");
        // PrintMathExpression(mathExpression1);
        // PrintMathExpression(mathExpression2);
        
        // testOutputHelper.WriteLine(mathTerm.StringRepresentation);
        // testOutputHelper.WriteLine(mathTerm2.StringRepresentation);
        Assert.Equal(mathExpression1.StringRepresentation, mathExpression2.StringRepresentation);
    }
}