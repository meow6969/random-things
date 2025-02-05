namespace factorizer.Models;

using Exceptions;
using static Latex.MathToLatex;
using static UtilityFunctions;

public class MathTerm // like "5yx^3"
{
    public Guid Id { get; } = Guid.NewGuid();
    // public int Coefficient { get; set; }
    // a math term is negative only if the coefficient is negative
    public MathVariable[] Variables { get; set; }
    public Dictionary<char, int> VariablesDict => MathTermVariablesToNameExponentDict(this);
    public int Coefficient { get; set; } = 1;
    
    public MathTerm(MathVariable[]? variables=null)
    {
        // Coefficient = coefficient;
        Variables = variables ?? [];
    }
    
    public void AddVariableToVariables(MathVariable variable)
    {
        List<MathVariable> newVariables = Variables.ToList();
        newVariables.Add(variable);
        Variables = newVariables.ToArray();
    }

    public MathVariable[] GetVariablesByName(char name, int? limit=null)
    {
        List<MathVariable> mathNumbers = [];
        
        foreach (MathVariable variable in Variables)
        {
            if (variable.Name != name) continue;
            mathNumbers.Add(variable);
            if (limit != null && mathNumbers.Count == limit) return mathNumbers.ToArray();
        }
        if (mathNumbers.Count == 0) throw new MathVariableNotFoundException(this, name);
        return mathNumbers.ToArray();
    }
    
    public MathVariable GetVariableById(Guid id)
    {
        foreach (MathVariable variable in Variables)
        {
            if (variable.Id == id) return variable;
        }
        throw new MathVariableNotFoundException(this, id);
    }

    public string StringRepresentation => MathTermToLatex(this);

    public KeyValuePair<char, MathVariable>[] AllMathNumberNames()
    {
        List<KeyValuePair<char, MathVariable>> mathNumbers = new List<KeyValuePair<char, MathVariable>>();
        foreach (MathVariable variable in Variables)
        {
            mathNumbers.Add(new KeyValuePair<char, MathVariable>(variable.Name, variable));
        }

        return mathNumbers.ToArray();
    }

    public MathTermFactors GetFactors()
    {
        return MathTermFactors.FromTerm(this);
    }
    
    public static Dictionary<char, int> MathTermVariablesToNameExponentDict(MathTerm term)
    {
        // we combine them here for reasons shut up ITS IMMPORTANT
        // ok brah why r u so mean to me  :c
        // true

        return MathVariable.MathVariablesToNameExponentDict(term.Variables);
    }
    
    public static int[] GetCoefficientCommonFactors(MathTermFactors[] factorsArray)
    {
        if (factorsArray.Length == 0) return [];
        if (factorsArray.Length == 1) return factorsArray[0].CoefficientFactors;

        List<int> commonFactors = [];
        MathTermFactors ogFactor = factorsArray[0];
        foreach (int factor in ogFactor.CoefficientFactors)
        {
            bool commonFactor = true;
            foreach (MathTermFactors numberFactor in factorsArray)
            {
                if (!numberFactor.CoefficientFactors.Contains(factor))
                {
                    commonFactor = false;
                }
            }
            
            if (commonFactor) commonFactors.Add(factor);
        }

        return commonFactors.ToArray();
    }
    
    public static KeyValuePair<bool, MathTerm[]> CombineMathTermsFromList(MathTerm[] combinedTerms)
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
    
    public static int[] GetCoefficientCommonFactors(List<MathTermFactors> factorsList)
    {
        return GetCoefficientCommonFactors(factorsList.ToArray());
    }
    
    public static MathTerm CombineMathTermMathNumbers(MathTerm term)
    {
        // Console.WriteLine(term.StringRepresentation);
        
        Dictionary<char, int> variableExponents = new Dictionary<char, int>();
        foreach (MathVariable number in term.Variables)
        {
            variableExponents.TryAdd(number.Name, 0);
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
    
    public static void PrintMathTerm(MathTerm term, int indent=0)
    {
        PrintWithIndent("new MathTerm:", indent, true);
        indent++;
        // Console.WriteLine($"term.StringRepresentation: {term.StringRepresentation}");
        PrintWithIndent($"term.Id: {term.Id.ToString()}", indent);
        PrintWithIndent($"term.Coefficient: {term.Coefficient}", indent);
        PrintWithIndent($"term.StringRepresentation: {term.StringRepresentation}", indent);
        // int varNum = 1;
        foreach (MathVariable number in term.Variables)
        {
            // PrintWithIndent($"\nVariable {varNum}: ", indent);
            MathVariable.PrintMathVariable(number, indent);
            // varNum++;
        }
    }
}

public class MathTermFactors
{
    public required int[] CoefficientFactors { get; init; }
    public required Tuple<int, int>[] CoefficientFactorPairs { get; init; }
    public required MathVariable[] VariableFactors { get; init; }
    
    public static MathTermFactors FromTerm(MathTerm term)
    {
        UtilityFunctions.NumberFactors termNumberFactors = UtilityFunctions.GetFactors(term.Coefficient);
        int[] coefficientFactors = termNumberFactors.Factors.ToArray();
        List<Tuple<int, int>> listPairs = [];
        foreach (List<int> pair in termNumberFactors.FactorPairs)
        {
            listPairs.Add(new Tuple<int, int>(pair[0], pair[1]));
        }

        return new MathTermFactors
        {
            CoefficientFactors = coefficientFactors,
            CoefficientFactorPairs = listPairs.ToArray(),
            VariableFactors = term.Variables
        };
    }
}









