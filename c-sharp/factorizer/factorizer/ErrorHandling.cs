using static factorizer.MathClasses;

namespace factorizer;

public abstract class ErrorHandling
{
    public class MathNumberNotFoundException : Exception
    {
        public MathNumberNotFoundException()
        {
        }

        public MathNumberNotFoundException(MathTerm term, Guid id) 
            : base($"Could not find math variable Id={id.ToString()} for math term {term.StringRepresentation}")
        {
        }
        
        public MathNumberNotFoundException(MathTerm term, char name) 
            : base($"Could not find math variable {name} for math term {term.StringRepresentation}")
        {
        }
    }
    
    public class MathTermNotFoundException : Exception
    {
        public MathTermNotFoundException()
        {
        }

        public MathTermNotFoundException(MathExpression term, Guid id) 
            : base($"Could not find math term Id={id.ToString()} for math term {term.StringRepresentation}")
        {
        }
    }

    public class LatexException : Exception
    {
        public LatexException()
        {
            
        }

        public LatexException(string latexExpression)
            : base($"invalid latex expression: {latexExpression}")
        {
        }
    }
}