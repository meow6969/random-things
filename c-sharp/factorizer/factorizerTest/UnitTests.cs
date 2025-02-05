using factorizer;
using factorizer.Models;
using static factorizer.Latex.LatexToMath;
using static factorizer.UtilityFunctions;
// ReSharper disable UnusedType.Global

namespace factorizerTest;

public class UnitTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestCombineMathExpressionMathTerms()
    {
        MathExpression mathExpression1 = LatexExpressionToMathExpression("-47x^{3}y+5y^{4}");
        MathExpression mathExpression2 = LatexExpressionToMathExpression("-57x^{2}xy+5x^{2}xy2+2y^{4}+3y^{4}");
        mathExpression2 = MathExpression.CombineMathExpressionMathTerms(mathExpression2);
        
        Assert.Equal(mathExpression1.StringRepresentation, mathExpression2.StringRepresentation);
    }

    [Fact]
    public void TestFactorRuleGreatestCommonFactor()
    {
        MathExpression mathExpression1 = LatexExpressionToMathExpression("10w^{3}+13w^{2}-3w");
        MathParentheses mathExpression2 = LatexParenthesesToMathParentheses("(w)(10w^{2}+13w-3)");
        
        
    }
    
    [Fact]
    public void TestMathExpressionCommonFactors()
    {
        MathExpression mathExpression1 = LatexExpressionToMathExpression("10w^{3}+15w^{2}-5w");
        // testOutputHelper.WriteLine(mathExpression1.StringRepresentation);
        MathExpressionCommonFactors commonFactors = MathExpressionCommonFactors.FromExpression(mathExpression1);
        string testText = $"{commonFactors.CoefficientCommonFactors[3]}, ";
        foreach (MathVariable mathVar in commonFactors.VariableCommonFactors)
        {
            testText += $"{mathVar.Name}^{mathVar.Exponent}";
            // testOutputHelper.WriteLine(mathVar.StringRepresentation);
        }

        Assert.Equal("5, w^1", testText);
    }
    
    [Fact]
    public void TestMathExpressionGreatestCommonFactor()
    {
        MathExpression mathExpression1 = LatexExpressionToMathExpression("10w^{3}+15w^{2}-5w");
        // testOutputHelper.WriteLine(mathExpression1.StringRepresentation);
        MathExpressionCommonFactors commonFactors = MathExpressionCommonFactors.FromExpression(mathExpression1);
        string testText = $"{commonFactors.CoefficientCommonFactors[3]}, ";
        foreach (MathVariable mathVar in commonFactors.VariableCommonFactors)
        {
            testText += $"{mathVar.Name}^{mathVar.Exponent}";
            // testOutputHelper.WriteLine(mathVar.StringRepresentation);
        }

        Assert.Equal("5, w^1", testText);
    }
    
    [Fact]
    public void TestFactoringRuleGreatestCommonFactor()
    {
        MathExpression mathExpression1 = LatexExpressionToMathExpression("10w^{3}+15w^{2}-5w");
        MathParentheses factored = FactoringRules.GreatestCommonFactor(mathExpression1);

        Assert.Equal("(+5w)(2w^{2}+3w-1)", factored.StringRepresentation);
    }
}