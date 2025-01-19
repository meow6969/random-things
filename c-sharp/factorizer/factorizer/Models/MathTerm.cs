namespace factorizer.Models;

using Exceptions;
using static Latex.MathToLatex;

public class MathTerm // like "5yx^3"
{
    public Guid Id { get; } = Guid.NewGuid();
    // public int Coefficient { get; set; }
    // a math term is negative only if the coefficient is negative
    public MathVariable[] Variables { get; set; }
    public Dictionary<char, int> VariablesDict => UtilityFunctions.MathTermVariablesToNameExponentDict(this);
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
}