using factorizer.Models;

namespace factorizer.Exceptions;

public class MathExpressionNotFoundException : Exception
{
    public MathExpressionNotFoundException()
    {
    }

    public MathExpressionNotFoundException(MathParentheses parentheses, Guid id) 
        : base($"Could not find math expression Id={id.ToString()} for math parentheses {parentheses.StringRepresentation}")
    {
    }
}