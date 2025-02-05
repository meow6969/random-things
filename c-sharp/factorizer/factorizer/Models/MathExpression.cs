using static factorizer.Latex.MathToLatex;
using static factorizer.UtilityFunctions;

using factorizer.Exceptions;

namespace factorizer.Models;

public class MathExpression
{
    public MathTerm[] Terms { get; set; }
    public string StringRepresentation => MathExpressionToLatex(this);
    public Guid Id { get; } = Guid.NewGuid();
        
    public MathExpression(params MathTerm[]? terms)
    {
        Terms = terms ?? [];
    }
        
    public void AddTermToTerms(MathTerm term)
    {
        List<MathTerm> newTerms = Terms.ToList();
        newTerms.Add(term);
        Terms = newTerms.ToArray();
    }
        
    public MathTerm GetTermById(Guid id)
    {
        foreach (MathTerm term in Terms)
        {
            if (term.Id == id) return term;
        }
        throw new MathTermNotFoundException(this, id);
    }
    
    public static void PrintMathExpression(MathExpression expression, int indent=0)
    {
        PrintWithIndent("new MathExpression:", indent, true);
        indent++;
        // Console.WriteLine($"term.StringRepresentation: {term.StringRepresentation}");
        PrintWithIndent($"expression.Id: {expression.Id.ToString()}", indent);
        PrintWithIndent($"expression.StringRepresentation: {expression.StringRepresentation}", indent);
        indent++;
        // int varNum = 1;
        foreach (MathTerm term in expression.Terms)
        {
            // PrintWithIndent($"Term {varNum}: ", indent, true);
            MathTerm.PrintMathTerm(term, indent);
            // varNum++;
        }
    }
    
    public static MathExpression CombineMathExpressionMathTerms(MathExpression expression)
    { 
        // bro i cant figure out wtf is happenign anymore 
        List<MathTerm> combinedTerms = [];
        
        // [      1,        2,        3]
        // [1, 2, 3] [1, 2, 3] [1, 2, 3]
        
        // this gives us a unique list of MathTerms
        combinedTerms.AddRange(expression.Terms.Select(MathTerm.CombineMathTermMathNumbers));

        bool combinedATerm = true;
        while (combinedATerm)
        {
            KeyValuePair<bool, MathTerm[]> temp = MathTerm.CombineMathTermsFromList(combinedTerms.ToArray());
            combinedATerm = temp.Key;
            combinedTerms = temp.Value.ToList();
        }

        return new MathExpression(combinedTerms.ToArray());
    }
}


public class MathExpressionCommonFactors
{
    public required int[] CoefficientCommonFactors { get; init; }
    public required MathVariable[] VariableCommonFactors { get; init; }
    public required Dictionary<char, int> VariableCommonFactorsDict { get ; init; }
    
    public static MathExpressionCommonFactors FromExpression(MathExpression expression)
    {
        // 10w^3+13w^2-3w
        List<MathTermFactors> termFactors = [];
        // w^3+w^2-w
        List<MathVariable[]> termVariables = [];

        foreach (MathTerm term in expression.Terms)
        {
            termFactors.Add(MathTermFactors.FromTerm(term));
            termVariables.Add(term.Variables);
        }
        
        // we have to compare every variable with every other variable
        Dictionary<char, int> commonVars = [];
        int i = 1;
        foreach (MathVariable[] mathVar in termVariables)
        {
            // Console.WriteLine(MathVariablesToLatex(mathVar));
            Dictionary<char, int> varVars = MathVariable.MathVariablesToNameExponentDict(mathVar);
        
            // we slice here because otherwise we would be comparing the same terms multiple times
            foreach (MathVariable[] mathVar2 in termVariables[i..])
            {
                Dictionary<char, int> term2Variables = MathVariable.MathVariablesToNameExponentDict(mathVar2);
                // PrintDict(term2Variables);
                foreach (KeyValuePair<char, int> meowVar in varVars)
                {
                    if (term2Variables.ContainsKey(meowVar.Key))
                    {
                        if (commonVars.ContainsKey(meowVar.Key))
                            commonVars[meowVar.Key] = ManyMin(
                                meowVar.Value, term2Variables[meowVar.Key], commonVars[meowVar.Key]);
                        else
                        {
                            commonVars[meowVar.Key] = Math.Min(meowVar.Value, term2Variables[meowVar.Key]);
                            // PrintDict(commonVars);
                            // Console.WriteLine(meowVar.Key);
                            // if (commonVars.ContainsKey(meowVar.Key)) commonVars.Remove(meowVar.Key);
                        }
                    }
                    else
                    {
                        if (commonVars.ContainsKey(meowVar.Key)) commonVars.Remove(meowVar.Key);
                    }
                }
            }

            i += 1;
        }

        return new MathExpressionCommonFactors
        {
            CoefficientCommonFactors = MathTerm.GetCoefficientCommonFactors(termFactors),
            VariableCommonFactorsDict = commonVars,
            VariableCommonFactors = MathVariable.MathVariablesFromNameExponentDict(commonVars)
        };
    }
}



