namespace Boids;

public static class Config
{
    public const int BoidSize = 4;
    public const int BoidSpeed = 2;
    public const int LocalBoidRange = 100;
    public const float SeparationWeight = 6f;
    public const float AlignmentWeight = 3f;
    public const float CohesionWeight = 5f;
    public const int BoidSteeringDivider = 10;
    public const int FrameRate = 30;
    public const int StartingBoids = 2000;
    public const float BoidEntropyMultiplier = 0.0f;
}