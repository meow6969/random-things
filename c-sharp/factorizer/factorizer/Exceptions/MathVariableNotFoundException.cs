using factorizer.Models;

namespace factorizer.Exceptions;

public class MathVariableNotFoundException : Exception
{
    public MathVariableNotFoundException()
    {
    }

    public MathVariableNotFoundException(MathTerm term, Guid id) 
        : base($"Could not find math variable Id={id.ToString()} for math term {term.StringRepresentation}")
    {
    }
    
    public MathVariableNotFoundException(MathTerm term, char name) 
        : base($"Could not find math variable {name} for math term {term.StringRepresentation}")
    {
    }
}