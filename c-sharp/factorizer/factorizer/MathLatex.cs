using static factorizer.MathClasses;
using static factorizer.UtilityFunctions;

namespace factorizer;

public abstract class MathLatex
{
    // public static readonly string[] MathOperations = ["\\cdot", "+", "-", "\\frac", "="];
    public static readonly string[] MathOperations = ["+", "-"];

    // 5y1x^{3}\cdot+-\frac{ }{ }=0
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
            term += variable.Name;
            if (variable.Exponent != 1) term += "^{" + $"{variable.Exponent}" + "}";
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

    public static MathTerm LatexTermToMathTerm(string latexTerm)
    {
        // will be like 5y^{69}x^{4}
        // this code just going to assumme whatever called it actually gave it a latexterm
        MathTerm mathTerm = new MathTerm();
        MathVariable? lastAddedVariable = null;

        bool gettingCoefficient = false;
        bool gettingExponent = false;
        bool negative = false;
        string token = "";
        foreach (char theChar in latexTerm)
        {
            if (theChar == '\\') throw new Exception($"LatexTermToMathTerm: expected only a term, not \\\n" +
                                                     $"latexTerm: {latexTerm}, token: {token}");
            token += theChar;
            if (MathOperations.Contains($"{theChar}"))
            {
                if (theChar == '+')
                {
                    token = "";
                    continue;
                }
                if (theChar == '-')
                {
                    gettingCoefficient = true;
                    negative = true;
                    token = "";
                    continue;
                }
            }
            
            if (token.All(char.IsNumber) && !gettingExponent)
            {
                gettingCoefficient = true;
            }
            else if (gettingCoefficient || negative)
            {
                // coefficients.Add(int.Parse(token.Substring(0, token.Length - 1)));
                mathTerm.Coefficient *= int.Parse(RemoveLastFromString(token));
                if (negative) mathTerm.Coefficient *= -1;
                negative = false;
                gettingCoefficient = false;
                token = $"{theChar}";
            }
            
            if (token.All(char.IsLetter))
            {
                lastAddedVariable = new MathVariable
                {
                    Name = theChar
                };
                
                mathTerm.AddVariableToVariables(lastAddedVariable);
                token = "";
                continue;
            }
            if (token == "^{")  // exponent!!
            {
                token = "";
                gettingExponent = true;
            }
            else if (gettingExponent && theChar == '}')
            {
                if (lastAddedVariable == null) throw new ErrorHandling.LatexException(latexTerm);
                gettingExponent = false;
                int exponent = Int32.Parse(RemoveLastFromString(token));
                lastAddedVariable.Exponent = exponent;
                token = "";
            }
        }
        
        if (int.TryParse(token, out int n)) mathTerm.Coefficient *= n;

        return mathTerm;
    }
    
    public static MathExpression LatexExpressionToMathExpression(string latexExpression)
    {
        // will be like 5y^{69}x^{4}+5x
        // this code just going to assumme whatever called it actually gave it a latexexpression
        MathExpression mathExpression = new MathExpression();

        string token = "";
        string currentOperation = "";
        bool gettingOperation = false;
        // bool gettingMathTerm = false;
        foreach (char theChar in latexExpression)
        {
            if (theChar == '\\')  // not implemented yet (this is for like fraction stuff)
            {
                gettingOperation = true;
                token = "";
            }
            else if (MathOperations.Contains($"{theChar}"))
            {
                if (token.Length > 0)
                {
                    if (currentOperation == "") currentOperation = "+";
                    mathExpression.AddTermToTerms(LatexTermToMathTerm(currentOperation + token));
                }
                currentOperation = $"{theChar}";
                token = "";
            }
            else if (gettingOperation && MathOperations.Contains(token))  // not implemented yet (this is for like fraction stuff)
            {
                currentOperation = token;
                token = "";
                gettingOperation = false;
                continue;
            }
            
            token += theChar;
        }

        if (token.Length > 0)
        {
            if (currentOperation == "") currentOperation = "+";
            MathTerm term = LatexTermToMathTerm(currentOperation + token);
            // Console.WriteLine($"currentOperation + token: {currentOperation}{token}");
            // Console.WriteLine("THE TERMM VVV");
            // PrintMathTerm(term);
            // Console.WriteLine("THE TERM ^^^^");
            mathExpression.AddTermToTerms(term);
        }

        return mathExpression;
    }
}