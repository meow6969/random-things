using factorizer.Models;

namespace factorizer;

public class FactoringRules
{
    // this is only for expressions
    // going to try to implement all (or most) of the ones from here https://www.mathwords.com/f/factoring_rules.htm

    public static MathParentheses GreatestCommonFactor(MathExpression expression)
    {
        MathExpressionCommonFactors commonFactors = MathExpressionCommonFactors.FromExpression(expression);
        List<MathTerm> newTerms = [];

        int greatestCoefficientCommonFactor = commonFactors.CoefficientCommonFactors.Max(x => x);
        
        foreach (MathTerm term in expression.Terms)
        {
            List<MathVariable> newVariables = [];
            term.Coefficient /= greatestCoefficientCommonFactor;
            foreach (MathVariable theVar in term.Variables)
            {
                if (commonFactors.VariableCommonFactorsDict.TryGetValue(theVar.Name, out var value))
                {
                    theVar.Exponent -= value;
                }

                newVariables.Add(theVar);
            }
            newTerms.Add(new MathTerm
            {
                Coefficient = term.Coefficient,
                Variables = newVariables.ToArray()
            });
        }

        return new MathParentheses
        {
            Coefficient = new MathTerm
            {
                Coefficient = greatestCoefficientCommonFactor,
                Variables = commonFactors.VariableCommonFactors
            },
            Expressions =
            [
                new MathExpression
                {
                    Terms = newTerms.ToArray()
                }
            ]
        };
    }
}