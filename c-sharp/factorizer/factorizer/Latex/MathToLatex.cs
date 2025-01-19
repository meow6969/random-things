using factorizer.Models;

namespace factorizer.Latex;

public class MathToLatex
{
    public static string MathVariableToLatex(MathVariable mathVariable)
    {
        string term = "";
        term += mathVariable.Name;
        if (mathVariable.Exponent != 1) term += "^{" + $"{mathVariable.Exponent}" + "}";
        return term;
    }
    
    public static string MathTermToLatex(MathTerm mathTerm)  
    {
        // 5yx^{3} summthing like dis
        string term = "";
        // if (mathTerm.Coefficient != 1) term += $"{mathTerm.Coefficient}";
        // PrintMathTerm(mathTerm);
        if (mathTerm.Coefficient >= 0) term += "+";
        if (mathTerm.Coefficient != 1) term += $"{mathTerm.Coefficient}";
        
        foreach (MathVariable variable in mathTerm.Variables)
        {
            term += MathVariableToLatex(variable);
        }

        return term;
    }

    public static string MathExpressionToLatex(MathExpression mathExpression)
    {
        // 5yx^{3}+3y summthing like dis
        string expression = "";
        // PrintMathExpression(mathExpression);
        
        foreach (MathTerm term in mathExpression.Terms)
        {
            expression += MathTermToLatex(term);
        }

        return expression;
    }
    
    public static string MathParenthesesToLatex(MathParentheses mathParentheses)
    {
        // (5yx^{3}+3y)(y+x) summthing like dis
        string parentheses = "";
        // PrintMathExpression(mathExpression);
        
        foreach (MathExpression expression in mathParentheses.Expressions)
        {
            parentheses += "\\left(" + MathExpressionToLatex(expression) + "\\right)";
        }

        return parentheses;
    }
}