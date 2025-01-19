using factorizer.Models;

namespace factorizer.Exceptions;

public class MathTermNotFoundException : Exception
{
    public MathTermNotFoundException()
    {
    }

    public MathTermNotFoundException(MathExpression expression, Guid id) 
        : base($"Could not find math term Id={id.ToString()} for math expression {expression.StringRepresentation}")
    {
    }
}