using System.Collections;
using factorizer;
using factorizer.Models;
// ReSharper disable MemberCanBePrivate.Global

namespace factorizer;

public abstract class UtilityFunctions
{
    public static string RemoveLastFromString(string text, int lastToRemove=1)
    {
        // Console.WriteLine(text);
        // Console.WriteLine(text.Substring(0, text.Length - lastToRemove));
        return text.Substring(0, text.Length - lastToRemove);
    }

    public static void PrintMathTerm(MathTerm term)
    {
        Console.WriteLine("\nnew MathTerm:");
        // Console.WriteLine($"term.StringRepresentation: {term.StringRepresentation}");
        Console.WriteLine($"term.Id: {term.Id.ToString()}");
        Console.WriteLine($"term.StringRepresentation: {term.StringRepresentation}");
        int varNum = 1;
        foreach (MathVariable number in term.Variables)
        {
            Console.WriteLine($"\nVariable {varNum}: ");
            PrintMathVariable(number);
            varNum++;
        }
    }

    public static void PrintMathExpression(MathExpression expression)
    {
        Console.WriteLine("\nnew MathExpression:");
        // Console.WriteLine($"term.StringRepresentation: {term.StringRepresentation}");
        Console.WriteLine($"expression.Id: {expression.Id.ToString()}");
        Console.WriteLine($"expression.StringRepresentation: {expression.StringRepresentation}");
        int varNum = 1;
        foreach (MathTerm term in expression.Terms)
        {
            Console.WriteLine($"\nTerm {varNum}: ");
            PrintMathTerm(term);
            varNum++;
        }
    }
    
    public static void PrintMathParenthesis(MathParentheses parenthesis)
    {
        Console.WriteLine("\nnew MathParenthesis:");
        // Console.WriteLine($"term.StringRepresentation: {term.StringRepresentation}");
        Console.WriteLine($"parenthesis.Id: {parenthesis.Id.ToString()}");
        Console.WriteLine($"parenthesis.StringRepresentation: {parenthesis.StringRepresentation}");
        int varNum = 1;
        foreach (MathExpression expression in parenthesis.Expressions)
        {
            Console.WriteLine($"\nTerm {varNum}: ");
            PrintMathExpression(expression);
            varNum++;
        }
    }

    // public static void PrintMathNumber(MathNumber mathNumber)
    // {
    //     Console.WriteLine($"mathNumber.Coefficient: {mathNumber.Coefficient}");
    //     Console.WriteLine($"mathNumber.Id: {mathNumber.Id}");
    //     Console.WriteLine($"mathNumber.Exponent: {mathNumber.Exponent}");
    //     if (mathNumber.Name == null) Console.WriteLine($"mathNumber.Name: null");
    //     else Console.WriteLine($"mathNumber.Name: {mathNumber.Name}");
    // }
    
    public static void PrintMathVariable(MathVariable mathNumber)
    {
        Console.WriteLine($"MathNumber.Id: {mathNumber.Id.ToString()}");
        Console.WriteLine($"MathNumber.Exponent: {mathNumber.Exponent}");
        Console.WriteLine($"MathNumber.Name: {mathNumber.Name}");
    }
    
    public static MathTerm CombineMathTermMathNumbers(MathTerm term)
    {
        // Console.WriteLine(term.StringRepresentation);
        
        Dictionary<char, int> variableExponents = new Dictionary<char, int>();
        foreach (MathVariable number in term.Variables)
        {
            if (!variableExponents.ContainsKey(number.Name)) variableExponents[number.Name] = 0;
            variableExponents[number.Name] += number.Exponent;
        }

        MathTerm newTerm = new MathTerm { Coefficient = term.Coefficient };
        foreach (char variableName in variableExponents.Keys)
        {
            newTerm.AddVariableToVariables(new MathVariable
            {
                Exponent = variableExponents[variableName],
                Name = variableName
            });
        }
        
        // Console.WriteLine(newTerm.StringRepresentation);

        return newTerm;
    }
    
    public static Dictionary<char, int> MathTermVariablesToNameExponentDict(MathTerm term)
    {
        Dictionary<char, int> termVariables = [];
        // we combine them here for reasons shut up ITS IMMPORTANT
        // ok brah why r u so mean to me  :c
        foreach (MathVariable variable in CombineMathTermMathNumbers(term).Variables)
        {
            termVariables[variable.Name] = variable.Exponent;
        }

        return termVariables;
    }

    public static MathExpression CombineMathExpressionMathTerms(MathExpression expression)
    { 
        // bro i cant figure out wtf is happenign anymore 
        List<MathTerm> combinedTerms = [];
        
        // [      1,        2,        3]
        // [1, 2, 3] [1, 2, 3] [1, 2, 3]
        
        // this gives us a unique list of MathTerms
        combinedTerms.AddRange(expression.Terms.Select(CombineMathTermMathNumbers));

        bool combinedATerm = true;
        while (combinedATerm)
        {
            KeyValuePair<bool, MathTerm[]> temp = CombineMathTermsFromList(combinedTerms.ToArray());
            combinedATerm = temp.Key;
            combinedTerms = temp.Value.ToList();
        }

        return new MathExpression(combinedTerms.ToArray());
    }
    
    private static KeyValuePair<bool, MathTerm[]> CombineMathTermsFromList(MathTerm[] combinedTerms)
    {
        bool combinedATerm = false;
        
        List<MathTerm> newTerms = [];
        List<Guid> doneTerms = [];
        
        // we have to compare every term with every other term
        int i = 0;
        foreach (MathTerm term1 in combinedTerms)
        {
            if (doneTerms.Contains(term1.Id)) continue;
            Dictionary<char, int> term1Variables = MathTermVariablesToNameExponentDict(term1);
            
            // we slice here because otherwise we would be comparing the same terms multiple times
            foreach (MathTerm term2 in combinedTerms[i..])
            {
                if (doneTerms.Contains(term2.Id)) continue;
                if (ReferenceEquals(term1, term2)) continue;
                Dictionary<char, int> term2Variables = MathTermVariablesToNameExponentDict(term2);
                if (!term1Variables.SequenceEqual(term2Variables)) continue;
                // that means we can add them mtogether!!!!!!!!
                MathTerm term2Replacement = new MathTerm
                {
                    Variables = term2.Variables,
                    Coefficient = term1.Coefficient + term2.Coefficient
                };
                combinedATerm = true;
                doneTerms.Add(term1.Id);
                doneTerms.Add(term2.Id);
                newTerms.Add(term2Replacement);
            }

            i += 1;
        }

        foreach (MathTerm term in combinedTerms)
        {
            if (!doneTerms.Contains(term.Id))
            {
                newTerms.Add(term);
            }
        }

        return new KeyValuePair<bool, MathTerm[]>(combinedATerm, newTerms.ToArray());
    }
}