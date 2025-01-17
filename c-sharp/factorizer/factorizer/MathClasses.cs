using static factorizer.MathLatex;
using static factorizer.ErrorHandling;

namespace factorizer;

public abstract class MathClasses
{
    public class MathNumber
    {
        public int Coefficient { get; set; } = 1;
        // TODO: allow for exponent to be algebraic expression for rn tho they are ints
        public int Exponent { get; set; } = 1;
        public Guid Id { get; } = Guid.NewGuid();
        public char? Name { get; set; }
        
        public MathNumber()
        {
            // note Coefficient is just the value of the number
            // it is this way for parity between the 2 types (MathNumber, MathNumber)
        }
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
        public MathNumber[] Variables { get; set; }
        public Dictionary<char, int> VariablesDict => UtilityFunctions.MathTermVariablesToNameExponentDict(this);
        public int Coefficient => UtilityFunctions.GetCoefficientFromMathTerm(this);
        
        public MathTerm(MathNumber[]? variables=null)
        {
            // Coefficient = coefficient;
            Variables = variables ?? [];
        }
        
        public void AddVariableToVariables(MathNumber variable)
        {
            List<MathNumber> newVariables = Variables.ToList();
            newVariables.Add(variable);
            Variables = newVariables.ToArray();
        }

        public MathNumber[] AllMathVariables()
        {
            List<MathNumber> mathNumbers = [];
            foreach (MathNumber number in Variables)
            {
                if (number.Name != null) mathNumbers.Add(number);
            }

            return mathNumbers.ToArray();
        }

        public MathNumber[] GetVariablesByName(char name, int? limit=null)
        {
            List<MathNumber> mathNumbers = [];
            
            foreach (MathNumber variable in AllMathVariables())
            {
                if (variable.Name != name) continue;
                mathNumbers.Add(variable);
                if (limit != null && mathNumbers.Count == limit) return mathNumbers.ToArray();
            }
            if (mathNumbers.Count == 0) throw new MathNumberNotFoundException(this, name);
            return mathNumbers.ToArray();
        }
        
        public MathNumber GetVariableById(Guid id)
        {
            foreach (MathNumber variable in Variables)
            {
                if (variable.Id == id) return variable;
            }
            throw new MathNumberNotFoundException(this, id);
        }

        public string StringRepresentation => MathTermToLatex(this);

        public KeyValuePair<char, MathNumber>[] AllMathNumberNames()
        {
            List<KeyValuePair<char, MathNumber>> mathNumbers = new List<KeyValuePair<char, MathNumber>>();
            foreach (MathNumber variable in AllMathVariables())
            {
                mathNumbers.Add(new KeyValuePair<char, MathNumber>((char)variable.Name!, variable));
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