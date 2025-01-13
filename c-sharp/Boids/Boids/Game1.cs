using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame;
using System.Threading.Tasks;

namespace Boids;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private List<Boid> _boids = [];
    private bool _mouseReleased = true;
    private int _windowWidth;
    private int _windowHeight;
    private List<int> _frameTimes = [];

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        TargetElapsedTime = TimeSpan.FromSeconds(1d / Config.FrameRate);
        Console.WriteLine($"TARGET FRAME TIME: {1d / Config.FrameRate}");
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _windowWidth = _graphics.GraphicsDevice.Viewport.Bounds.Width;
        _windowHeight = _graphics.GraphicsDevice.Viewport.Bounds.Height;
        Console.WriteLine($"{_windowWidth}, {_windowHeight}");
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        for (int i = 0; i < Config.StartingBoids; i++)
        {
            Random rnd = new Random();
            _boids.Add(new Boid(rnd.Next(0, _windowWidth), rnd.Next(0, _windowHeight), Config.BoidSize, Config.BoidSpeed, _spriteBatch));
        }
    }

    protected override void Update(GameTime gameTime)
    {
        
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        if (Mouse.GetState().LeftButton == ButtonState.Pressed && _mouseReleased)
        {
            _mouseReleased = false;
            int x = Mouse.GetState().X;
            int y = Mouse.GetState().Y;
            _boids.Add(new Boid(x, y, Config.BoidSize, Config.BoidSpeed, _spriteBatch));
        }
        else if (Mouse.GetState().LeftButton == ButtonState.Released) _mouseReleased = true;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();

        List<Task> targetVectors = [];
        foreach (Boid boid in _boids)
        {
            Task targetVector = Utils.GetTargetVectorAsync(_boids, boid, _windowWidth, _windowHeight);
            targetVectors.Add(targetVector);
        }

        Task.WhenAll(targetVectors.ToArray());
        foreach (Boid boid in _boids)
        {
            boid.Draw();
        }
        
        _spriteBatch.End();
        base.Draw(gameTime);
        _frameTimes.Add(gameTime.ElapsedGameTime.Milliseconds);
        if (_frameTimes.Count == Config.FrameRate)
        {
            int averageFrameTime = 0;
            foreach (int i in _frameTimes)
            {
                averageFrameTime += i;
            }

            averageFrameTime /= _frameTimes.Count;
            Console.WriteLine($"average frame time: {averageFrameTime / 1000d}");
            _frameTimes = [];
        }
    }
}