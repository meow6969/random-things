using static factorizer.MathLatex;
using static factorizer.ErrorHandling;

namespace factorizer;

public abstract class MathClasses
{
    public class MathVariable
    {
        // public int Coefficient { get; set; } = 1;
        // TODO: allow for exponent to be algebraic expression for rn tho they are ints
        public int Exponent { get; set; } = 1;
        public Guid Id { get; } = Guid.NewGuid();
        public char Name { get; set; }
    }
    
    // public class MathNumber : MathNumber // like x^3
    // {
    //     public new char Name { get; set; }
    //     public MathNumber()
    //     {
    //         if (MathTerm == null) Id = -1;
    //         else Id = MathTerm.GetUniqueId();
    //     }
    // }
    
    public class MathTerm  // like "5yx^3"
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
            if (mathNumbers.Count == 0) throw new MathNumberNotFoundException(this, name);
            return mathNumbers.ToArray();
        }
        
        public MathVariable GetVariableById(Guid id)
        {
            foreach (MathVariable variable in Variables)
            {
                if (variable.Id == id) return variable;
            }
            throw new MathNumberNotFoundException(this, id);
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
    
    public class MathExpression
    {
        public MathTerm[] Terms { get; set; }
        public string StringRepresentation => MathExpressionToLatex(this);
        public Guid Id { get; } = Guid.NewGuid();
        
        public MathExpression(params MathTerm[]? terms)
        {
            terms ??= [];
            Terms = terms;
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
    }
}