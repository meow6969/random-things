using System.Collections;
using factorizer;
using static factorizer.MathClasses;

namespace factorizer;

public class UtilityFunctions
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
        foreach (MathNumber number in term.Variables)
        {
            Console.WriteLine($"\nVariable {varNum}: ");
            PrintMathNumber(number);
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

    // public static void PrintMathNumber(MathNumber mathNumber)
    // {
    //     Console.WriteLine($"mathNumber.Coefficient: {mathNumber.Coefficient}");
    //     Console.WriteLine($"mathNumber.Id: {mathNumber.Id}");
    //     Console.WriteLine($"mathNumber.Exponent: {mathNumber.Exponent}");
    //     if (mathNumber.Name == null) Console.WriteLine($"mathNumber.Name: null");
    //     else Console.WriteLine($"mathNumber.Name: {mathNumber.Name}");
    // }
    
    public static void PrintMathNumber(MathNumber mathNumber)
    {
        Console.WriteLine($"MathNumber.Coefficient: {mathNumber.Coefficient}");
        Console.WriteLine($"MathNumber.Id: {mathNumber.Id.ToString()}");
        Console.WriteLine($"MathNumber.Exponent: {mathNumber.Exponent}");
        if (mathNumber.Name == null) Console.WriteLine($"MathNumber.Name: null");
        else Console.WriteLine($"MathNumber.Name: {mathNumber.Name}");
    }
    
    public static MathTerm CombineMathTermMathNumbers(MathTerm term)
    {
        Dictionary<char, int> variableExponents = new Dictionary<char, int>();
        int coefficients = term.Coefficient;
        foreach (MathNumber number in term.Variables)
        {
            if (number.Name != null)
            {
                variableExponents[(char)number.Name] += 1;
            }
        }

        MathTerm newTerm = new MathTerm();
        foreach (char variableName in variableExponents.Keys)
        {
            newTerm.AddVariableToVariables(new MathNumber
            {
                Coefficient = coefficients,
                Exponent = variableExponents[variableName],
                Name = variableName
            });
            coefficients = 1;
        }

        if (variableExponents.Count == 0)
        {
            newTerm.AddVariableToVariables(new MathNumber
            {
                Coefficient = coefficients
            });
        }

        return newTerm;
    }

    public static int GetCoefficientFromMathTerm(MathTerm term)
    {
        int coefficient = 0;
        
        foreach (MathNumber num in term.Variables)
        {
            coefficient *= (int)Math.Pow(num.Coefficient, num.Exponent);
        }

        return coefficient;
    }
    
    public static Dictionary<char, int> MathTermVariablesToNameExponentDict(MathTerm term)
    {
        Dictionary<char, int> termVariables = [];
        // we combine them here for reasons shut up ITS IMMPORTANT
        // ok brah why r u so mean to me  :c
        foreach (MathNumber variable in CombineMathTermMathNumbers(term).Variables)
        {
            if (variable.Name == null) continue;
            termVariables[(char)variable.Name] = variable.Exponent;
        }

        return termVariables;
    }

    public MathNumber[] CombineLikeVariables(MathTerm term1, MathTerm term2)
    {
        MathNumber[] variables = [];
        
    }

    public MathExpression CombineMathExpressionMathTerms(MathExpression expression)
    { 
        // bro i cant figure out wtf is happenign anymore 
        List<MathTerm> combinedTerms = [];
        
        // [      1,        2,        3]
        // [1, 2, 3] [1, 2, 3] [1, 2, 3]
        
        // this gives us a unique list of MathTerms
        combinedTerms.AddRange(expression.Terms.Select(CombineMathTermMathNumbers));

        List<Guid> doneTerms = [];
        Dictionary<Guid, MathTerm> newTerms = [];
        
        // we have to compare every term with every other term
        int i = 0;
        foreach (MathTerm term1 in combinedTerms)
        {
            Dictionary<char, int> term1Variables = MathTermVariablesToNameExponentDict(term1);
            
            // we slice here because otherwise we would be comparing the same terms multiple times
            foreach (MathTerm term2 in combinedTerms[i..])
            {
                if (ReferenceEquals(term1, term2)) continue;
                Dictionary<char, int> term2Variables = MathTermVariablesToNameExponentDict(term2);
                if (!term1Variables.SequenceEqual(term2Variables)) continue;
                // that means we can add them mtogether!!!!!!!!
                MathTerm term2Replacement = new MathTerm
                {
                    Variables = 
                };
                doneTerms.Add(term1.Id);
                newTerms.Add(term2.Id, term2Replacement);
            }

            i += 1;
        }
    }
}