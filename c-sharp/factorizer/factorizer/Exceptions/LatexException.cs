namespace factorizer.Exceptions;

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