using static factorizer.Latex.MathToLatex;

using factorizer.Exceptions;

namespace factorizer.Models;

public class MathParentheses
{
    public MathExpression[] Expressions { get; set; }
    public string StringRepresentation => MathParenthesesToLatex(this);
    public Guid Id { get; } = Guid.NewGuid();
        
    public MathParentheses(params MathExpression[]? expressions)
    {
        Expressions = expressions ?? [];
    }
        
    public void AddParenthesisToParentheses(MathExpression expression)
    {
        List<MathExpression> newExpressions = Expressions.ToList();
        newExpressions.Add(expression);
        Expressions = newExpressions.ToArray();
    }
        
    public MathExpression GetTermById(Guid id)
    {
        foreach (MathExpression term in Expressions)
        {
            if (term.Id == id) return term;
        }
        throw new MathExpressionNotFoundException(this, id);
    }
}