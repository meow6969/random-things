namespace factorizer.Latex;

using Models;
using Exceptions;
using static UtilityFunctions;

public class LatexToMath
{
    // public static readonly string[] MathOperations = ["\\cdot", "+", "-", "\\frac", "="];
    public static readonly string[] MathOperations = ["+", "-"];

    // 5y1x^{3}\cdot+-\frac{ }{ }=0
    

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
                if (lastAddedVariable == null) throw new LatexException(latexTerm);
                gettingExponent = false;
                int exponent = Int32.Parse(RemoveLastFromString(token));
                lastAddedVariable.Exponent = exponent;
                token = "";
            }
        }
        
        if (int.TryParse(token, out int n)) mathTerm.Coefficient *= n;
        if (negative) mathTerm.Coefficient *= -1;

        return mathTerm;
    }
    
    public static MathExpression LatexExpressionToMathExpression(string latexExpression)
    {
        // will be like 5y^{69}x^{4}+5x
        // this code just going to assumme whatever called it actually gave it a latexexpression
        MathExpression mathExpression = new MathExpression();
        // Console.WriteLine(latexExpression);

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
            // if (currentOperation == "") currentOperation = "+";
            // Console.WriteLine($"token: {token}");
            MathTerm term = LatexTermToMathTerm(token);
            // Console.WriteLine($"currentOperation + token: {currentOperation}{token}");
            // Console.WriteLine("THE TERMM VVV");
            // PrintMathTerm(term);
            // Console.WriteLine("THE TERM ^^^^");
            mathExpression.AddTermToTerms(term);
        }

        return mathExpression;
    }

    enum SearchingFor
    {
        Nothing,
        Parenthesis,
        StartParenthesis,
        EndParenthesis
    }

    // like: \left(6x+9\right)\left(9y+7\right)
    // TODO: support the coefficient thing like: 6x+9(5x+5) -> 6x, 9(5x+5)
    // TODO:                         rn it goes: 6x+9(5x+5) -> (6x+9)(5x+5), which is wrong
    public static MathParentheses LatexParenthesesToMathParentheses(string latexParentheses)
    {
        string startParenthesis = "\\left(";
        string endParenthesis = "\\right)";
        SearchingFor searchingFor = SearchingFor.Nothing;
        List<string> tokens = [];
        string token = "";
        string searchingForToken = "";

        foreach (char theChar in latexParentheses)
        {
            // Console.WriteLine(theChar);
            if (theChar == '\\')
            {
                searchingFor = SearchingFor.Parenthesis;
                searchingForToken += theChar;
                continue;
            }

            if (searchingFor > 0) // if its trying to search for something (nothing=0)
            {
                // Console.WriteLine($"theChar : {theChar}");
                // Console.WriteLine($"searchingFor: {searchingFor}");
                // Console.WriteLine($"searchingForToken: {searchingForToken}\n");
                
                searchingForToken += theChar;
                if (startParenthesis.StartsWith(searchingForToken))
                {
                    if (searchingForToken == startParenthesis)
                    {
                        // Console.WriteLine($"{searchingForToken} = {startParenthesis}");
                        searchingFor = SearchingFor.Nothing;
                        tokens.Add(token);
                        // Console.WriteLine(token);
                        token = "";
                        searchingForToken = "";
                    }
                    else
                    {
                        searchingFor = SearchingFor.StartParenthesis;
                    }
                    continue;
                }
                else if (endParenthesis.StartsWith(searchingForToken))
                {
                    if (searchingForToken == endParenthesis)
                    {
                        searchingFor = SearchingFor.Nothing;
                        tokens.Add(token);
                        token = "";
                        searchingForToken = "";
                    }
                    else
                    {
                        searchingFor = SearchingFor.EndParenthesis;
                    }
                    continue;
                }
                else // its not looking for a parenthesis so we just ignore it
                {
                    token += searchingForToken;
                    searchingForToken = "";
                    searchingFor = SearchingFor.Nothing;
                    continue;
                }
            }
            
            token += theChar;
            // Console.WriteLine($"token: {token}");
            // Console.ReadLine();
        }

        List<MathExpression> mathExpressions = [];

        foreach (string newToken in tokens)
        {
            if (newToken.Trim() == "") continue;
            // Console.WriteLine(newToken);
            mathExpressions.Add(LatexExpressionToMathExpression(newToken));
        }
        return new MathParentheses(mathExpressions.ToArray()); 
    }
}