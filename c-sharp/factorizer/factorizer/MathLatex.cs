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
        bool firstPass = true;
        // PrintMathTerm(mathTerm);
        
        foreach (MathNumber variable in mathTerm.Variables)
        {
            if (firstPass && variable.Coefficient >= 0) term += "+";
            if (variable.Coefficient != 1) term += variable.Coefficient;
            if (variable is { Exponent: 1, Coefficient: 1, Name: null }) term += "1";
            if (variable.Name != null) term += variable.Name;
            if (variable.Exponent != 1) term += "^{" + $"{variable.Exponent}" + "}";
            
            firstPass = false;
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
        MathNumber? lastAddedVariable = null;

        int currentCoefficient = 1;
        bool gettingCoefficient = false;
        bool gettingExponent = false;
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
                else if (theChar == '-')
                {
                    gettingCoefficient = true;
                    token = "-1";
                    continue;
                }
            }
            
            if (int.TryParse(token, out int _))
            {
                if (gettingExponent) continue;
                gettingCoefficient = true;
                continue;
            }
            else if (gettingCoefficient)
            {
                // coefficients.Add(int.Parse(token.Substring(0, token.Length - 1)));
                currentCoefficient = int.Parse(RemoveLastFromString(token));
                token = $"{theChar}";
                gettingCoefficient = false;
            }
            
            if (token.All(Char.IsLetter))
            {
                lastAddedVariable = new MathNumber
                {
                    Name = char.Parse(token),
                    Coefficient = currentCoefficient,
                    Exponent = 1
                };
                currentCoefficient = 1;
                
                mathTerm.AddVariableToVariables(lastAddedVariable);
                token = "";
                continue;
            }

            if (token == "^{")  // exponent!!
            {
                token = token.Substring(2);
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
        
        if (int.TryParse(token, out int n))
        {
            lastAddedVariable = new MathNumber
            {
                Coefficient = n
            };
                
            mathTerm.AddVariableToVariables(lastAddedVariable);
        }

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
        bool gettingMathTerm = false;
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