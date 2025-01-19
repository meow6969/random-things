using static factorizer.Latex.MathToLatex;

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
}