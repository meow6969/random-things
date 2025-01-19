namespace factorizer.Models;

public class MathVariable // like y^2
{
    // public int Coefficient { get; set; } = 1;
    // TODO: allow for exponent to be algebraic expression for rn tho they are ints
    public int Exponent { get; set; } = 1;
    public Guid Id { get; } = Guid.NewGuid();
    public char Name { get; set; }
}