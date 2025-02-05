using static factorizer.Latex.MathToLatex;
using static factorizer.UtilityFunctions;

using factorizer.Exceptions;

namespace factorizer.Models;

public class MathParentheses
{
    public MathExpression[] Expressions { get; set; }
    public string StringRepresentation => MathParenthesesToLatex(this);
    public Guid Id { get; } = Guid.NewGuid();
    public MathTerm Coefficient = new MathTerm();
        
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
        
    public MathExpression GetExpresionById(Guid id)
    {
        foreach (MathExpression term in Expressions)
        {
            if (term.Id == id) return term;
        }
        throw new MathExpressionNotFoundException(this, id);
    }
    
    public static void PrintMathParentheses(MathParentheses parenthesis, int indent=0)
    {
        PrintWithIndent("\nnew MathParenthesis:", indent);
        indent++;
        // Console.WriteLine($"term.StringRepresentation: {term.StringRepresentation}");
        PrintWithIndent($"parenthesis.Id: {parenthesis.Id.ToString()}", indent);
        PrintWithIndent($"parenthesis.StringRepresentation: {parenthesis.StringRepresentation}", indent);
        // int varNum = 1;
        foreach (MathExpression expression in parenthesis.Expressions)
        {
            // PrintWithIndent($"Expression {varNum}: ", indent, true);
            MathExpression.PrintMathExpression(expression, indent); 
            // varNum++;
        }
    }
}