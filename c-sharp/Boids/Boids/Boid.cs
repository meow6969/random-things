using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace Boids;

public class Boid
{
    public Vector2 Position;
    public int Size;
    public int Speed;
    private SpriteBatch _spriteBatch;
    public Vector2 Acceleration;
    public Vector2 Velocity;
    public Color RectColor;
    
    public Boid(int x, int y, int size, int speed, SpriteBatch spriteBatch)
    {
        Position = new Vector2(x, y);
        Size = size;
        Speed = speed;
        _spriteBatch = spriteBatch;
        Random rnd = new Random();
        // Acceleration = new Vector2((float)rnd.NextDouble(), (float)rnd.NextDouble());
        Acceleration = Vector2.One;
        Velocity = Acceleration;
        RectColor = new Color(rnd.Next(100, 255), rnd.Next(100, 255), rnd.Next(100, 255));
    }

    public void Draw()
    {
        Velocity += Acceleration / Config.BoidSteeringDivider;
        Position += Velocity * Speed;
        Velocity.Normalize();
        Velocity += new Vector2(-Velocity.X, -Velocity.Y) * Config.BoidEntropyMultiplier;
        Acceleration = Vector2.Zero;
        _spriteBatch.FillRectangle(new Rectangle((int)Position.X, (int)Position.Y, Size, Size), RectColor);
    }
}