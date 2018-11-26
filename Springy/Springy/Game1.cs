using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using System.Collections.Generic;
using System;
using FarseerPhysics.Dynamics.Joints;

namespace Springy
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        List<DrawablePhysicsObject> crateList;
        DrawablePhysicsObject floor;
        KeyboardState prevKeyboardState;
        Random random;
        World world;
        List<DrawablePhysicsObject> paddles;

        DistanceJoint l;

        const float unitToPixel = 100.0f;
        const float pixelToUnit = 1 / unitToPixel;
        Body body;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            paddles = new List<DrawablePhysicsObject>();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            world = new World(new Vector2(0, 9.81f));

            Vector2 size = new Vector2(50, 50);
            body = BodyFactory.CreateRectangle(world, size.X * pixelToUnit, size.Y * pixelToUnit, 1);
            body.BodyType = BodyType.Dynamic;
            body.Position = new Vector2((GraphicsDevice.Viewport.Width / 2.0f) * pixelToUnit, 0);

            random = new Random();

            floor = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Floor"), new Vector2(GraphicsDevice.Viewport.Width, 100.0f), 1000);
            floor.Position = new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height - 50);
            floor.body.BodyType = BodyType.Static;
            floor.body.Rotation = 0.0f;
            crateList = new List<DrawablePhysicsObject>();
            prevKeyboardState = Keyboard.GetState();


            DrawablePhysicsObject simplePaddle = new DrawablePhysicsObject(
                world,
                Content.Load<Texture2D>("Paddle"),
                new Vector2(128, 16),
                10
            );
            simplePaddle.body.BodyType = BodyType.Static;
            simplePaddle.Position = new Vector2(150, 200);
            paddles.Add(simplePaddle);


            {
                DrawablePhysicsObject motorPaddle = new DrawablePhysicsObject(
                    world,
                    Content.Load<Texture2D>("Paddle"),
                    new Vector2(128, 16),
                    10
                    );

                Body motorPaddleAxle = BodyFactory.CreateCircle(world, 0.1f, 10f);
                // motorPaddle.body = new Body(world, CoordinateHelper.ToWorld( new Vector2(10)));
                var j = JointFactory.CreateRevoluteJoint
                    (
                        world,
                        motorPaddle.body,
                        motorPaddleAxle,
                         CoordinateHelper.ToWorld(new Vector2(64f, 10f)),
                         CoordinateHelper.ToWorld(new Vector2(400.0f, 200.0f))
                        );

                // set speed and torque  
                j.CollideConnected = false;
                j.MotorSpeed = MathHelper.Pi;
                j.MotorImpulse = 100;
                j.MotorEnabled = true;
                j.MaxMotorTorque = 100;
                //  paddles.Add(motorPaddle);

            }
            DrawablePhysicsObject trampolinePaddle = new DrawablePhysicsObject
            (
                world,
                Content.Load<Texture2D>("Paddle"),
                new Vector2(128, 16),
                10
            );
            trampolinePaddle.body.BodyType = BodyType.Dynamic;
            trampolinePaddle.Position = new Vector2(150, 150);
            trampolinePaddle.body.Rotation = 0;

            l = JointFactory.CreateDistanceJoint
            (
                world,
                simplePaddle.body,
                trampolinePaddle.body,
                simplePaddle.Position,                
                trampolinePaddle.Position              
            );
            l.CollideConnected = false;
            l.Length = 100f;
            l.Frequency = 1f;
            l.DampingRatio = 0.5f;

            paddles.Add(trampolinePaddle);

            // l.Frequency = 2.0f;
            //  l.DampingRatio = 0.1f;

            //var r = JointFactory.CreateDistanceJoint
            //(
            //    world,
            //    floor.body,
            //    trampolinePaddle.body,
            //    CoordinateHelper.ToWorld(trampolinePaddle.Position + new Vector2(64, 0)),
            //    CoordinateHelper.ToWorld(Vector2.UnitY)
            //);

            //r.CollideConnected = true;
            //r.Frequency = -2.0f;
            //r.DampingRatio = 0.05f;

            //world.AddJoint(l);
            // world.AddJoint(r);

            //paddles.Add(trampolinePaddle);

            // TODO: use this.Content to load your game content here
        }
        private void SpawnCrate()
        {
            DrawablePhysicsObject crate;
            crate = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Crate"), new Vector2(50.0f, 50.0f), 0.1f);
            crate.Position = new Vector2(random.Next(50, GraphicsDevice.Viewport.Width - 50), 1);

            crateList.Add(crate);
        }
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Space) && !prevKeyboardState.IsKeyDown(Keys.Space))
            {
                SpawnCrate();
            }
            MouseState state = Mouse.GetState();

            //try
            //{
            //    l.WorldAnchorA = CoordinateHelper.ToWorld(new Vector2(state.X, state.Y));
            //    l.WorldAnchorB = CoordinateHelper.ToWorld(new Vector2(state.X, state.Y));
            //}
            //catch (Exception)
            //{

            //    throw;
            //}

            prevKeyboardState = keyboardState;
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            foreach (DrawablePhysicsObject crate in crateList)
            {
                crate.Draw(spriteBatch);
            }
            foreach (DrawablePhysicsObject paddle in paddles)
            {
                paddle.Draw(spriteBatch);
            }
            floor.Draw(spriteBatch);
            spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
