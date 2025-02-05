using static factorizer.Latex.MathToLatex;
using static factorizer.UtilityFunctions;

namespace factorizer.Models;


public class MathVariable // like y^2
{
    // public int Coefficient { get; set; } = 1;
    // TODO: allow for exponent to be algebraic expression for rn tho they are ints
    public int Exponent { get; set; } = 1;
    public Guid Id { get; } = Guid.NewGuid();
    public char Name { get; set; }
    public string StringRepresentation => MathVariableToLatex(this);
    
    public static Dictionary<char, int> MathVariablesToNameExponentDict(MathVariable[] mathVariables)
    {
        Dictionary<char, int> termVariables = [];
        // we combine them here for reasons shut up ITS IMMPORTANT
        // ok brah why r u so mean to me  :c
        // true
        foreach (MathVariable variable in mathVariables)
        {
            termVariables[variable.Name] = variable.Exponent;
        }

        return termVariables;
    }

    public static MathVariable[] MathVariablesFromNameExponentDict(Dictionary<char, int> theDict)
    {
        List<MathVariable> theMathVariables = [];
        
        foreach (KeyValuePair<char, int> theVar in theDict)
        {
            theMathVariables.Add(new MathVariable
            {
                Name = theVar.Key,
                Exponent = theVar.Value
            });
        }
        return theMathVariables.ToArray();
    }
    
    public static void PrintMathVariable(MathVariable mathVariable, int indent=0)
    {
        PrintWithIndent("new MathVariable:", indent, true);
        indent++;
        PrintWithIndent($"MathVariable.Id: {mathVariable.Id.ToString()}", indent);
        PrintWithIndent($"MathVariable.Exponent: {mathVariable.Exponent}", indent);
        PrintWithIndent($"MathVariable.Name: {mathVariable.Name}", indent);
    }
}