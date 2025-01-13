using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Boids;

public static class Utils
{
    public static List<Boid> GetLocalBoids(List<Boid> boids, Boid boid)
    {
        List<Boid> localBoids = [];
        foreach (Boid localBoid in boids)
        {
            if (localBoid == boid) continue;
            if (Vector2.Distance(boid.Position, localBoid.Position) < Config.LocalBoidRange)
            {
                localBoids.Add(localBoid);
            }
        }
        
        return localBoids;
    }

    public static Vector2 GetAverageBoidPosition(List<Boid> localBoids)
    {
        Vector2 averageBoidPosition = Vector2.Zero;
        foreach (Boid boid in localBoids)
        {
            averageBoidPosition += boid.Position;
        }

        averageBoidPosition /= localBoids.Count;
        return averageBoidPosition;
    }

    public static Vector2 GetAverageBoidDirectionVector(List<Boid> localBoids)
    {
        Vector2 averageDirection = Vector2.Zero;
        foreach (Boid boid in localBoids)
        {
            averageDirection += boid.Velocity;
        }
        averageDirection.Normalize();
        return averageDirection;
    }
    
    public static Vector2 GetSeparationTargetVector(List<Boid> localBoids, Boid boid)
    {
        Vector2 averageBoidDirection = Vector2.Zero;
        foreach (Boid localBoid in localBoids)
        {
            averageBoidDirection += (boid.Position - localBoid.Position) / (float)Math.Pow(Vector2.Distance(boid.Position, localBoid.Position), 2);
        }
        
        // averageBoidDirection /= localBoids.Count;
        averageBoidDirection.Normalize();
        return averageBoidDirection;
    }

    public static Vector2 GetAlignmentTargetVector(List<Boid> localBoids, Boid boid)
    {
        return GetAverageBoidDirectionVector(localBoids);
    }

    public static Vector2 GetCohesionTargetVector(List<Boid> localBoids, Boid boid)
    {
        Vector2 averageBoidPosition = GetAverageBoidPosition(localBoids);
        averageBoidPosition -= boid.Position;
        averageBoidPosition.Normalize();
        return averageBoidPosition;
    }

    public static Task GetTargetVectorAsync(List<Boid> boids, Boid boid, int windowWidth, int windowHeight)
    {
        return Task.Run(() =>
        {
            Vector2 targetVector = GetTargetVector(boids, boid);
            
            // Console.WriteLine(targetVector);
            boid.Acceleration += targetVector / Config.BoidSteeringDivider;
            if (boid.Position.X < 0 || boid.Position.X > windowWidth - Config.BoidSize)
            {
                boid.Velocity.X *= -1;
            }
            if (boid.Position.Y < 0 || boid.Position.Y > windowHeight - Config.BoidSize)
            {
                boid.Velocity.Y *= -1;
            }
        });
    }

    public static Vector2 GetTargetVector(List<Boid> boids, Boid boid)
    {
        Vector2 targetVector = Vector2.Zero;
        List<Boid> localBoids = GetLocalBoids(boids, boid);
        if (localBoids.Count == 0) return boid.Acceleration;

        Vector2 separationTargetVector = GetSeparationTargetVector(localBoids, boid);
        Vector2 alignmentTargetVector = GetAlignmentTargetVector(localBoids, boid);
        Vector2 cohesionTargetVector = GetCohesionTargetVector(localBoids, boid);
        
        targetVector += separationTargetVector * Config.SeparationWeight;
        targetVector += alignmentTargetVector * Config.AlignmentWeight;
        targetVector += cohesionTargetVector * Config.CohesionWeight;

        return targetVector;
    }
}