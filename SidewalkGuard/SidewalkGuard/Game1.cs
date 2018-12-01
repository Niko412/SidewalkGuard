using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using System.Collections.Generic;
using System;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework.Audio;

namespace SidewalkFuard
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        const int HEIGHT = 700, WIDTH = 350;
        float timer = 3f, levelTimer = 3f;
        GraphicsDeviceManager graphicsDeviceManager;
        SpriteBatch spriteBatch;
        DrawablePhysicsObject floor,slider;
        Random random;
        World world;
        List<DrawablePhysicsObject> paddles,boxes;
        private int lives = 10, score = 0;
        SpriteFont spriteFont;
        Texture2D youLose;
        Cube cube = new Cube();
        const float unitToPixel = 100.0f, pixelToUnit = 1 / unitToPixel;
        Body body;
        SoundEffect hitSound, missSound, backgroundSound, loseSound, pouseOn, pouseOff;
        SoundEffectInstance soundEffectInstance;
        private bool didSpawn = false, isPaused = false, prevKeyboardState = false, currKeyboardState = false, isStarted = false;

        public Game1()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
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
            graphicsDeviceManager.PreferredBackBufferWidth = WIDTH;  // set this value to the desired width of your window
            graphicsDeviceManager.PreferredBackBufferHeight = HEIGHT;   // set this value to the desired height of your window
            graphicsDeviceManager.ApplyChanges();
            paddles = new List<DrawablePhysicsObject>();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            backgroundSound = Content.Load<SoundEffect>("background_music");
            soundEffectInstance = backgroundSound.CreateInstance();
            soundEffectInstance.IsLooped = true;
            soundEffectInstance.Play();
            hitSound = Content.Load<SoundEffect>("hit");
            missSound = Content.Load<SoundEffect>("miss");
            loseSound = Content.Load<SoundEffect>("boo_sound");
            youLose = Content.Load<Texture2D>("you_lose_sticker");
            pouseOff = Content.Load<SoundEffect>("pouseOff");
            pouseOn = Content.Load<SoundEffect>("pouseOn");

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            world = new World(new Vector2(0, 9.81f));
            spriteFont = Content.Load<SpriteFont>("font");
            Vector2 size = new Vector2(50, 50);
            body = BodyFactory.CreateRectangle(world, size.X * pixelToUnit, size.Y * pixelToUnit, 1);
            body.BodyType = BodyType.Dynamic;
            body.Position = new Vector2((GraphicsDevice.Viewport.Width / 2.0f) * pixelToUnit, 0);


            random = new Random();

            floor = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Floor"), new Vector2(GraphicsDevice.Viewport.Width, 100.0f), 1000);
            floor.Position = new Vector2(GraphicsDevice.Viewport.Width / 2.0f, GraphicsDevice.Viewport.Height - 50);
            floor.body.BodyType = BodyType.Static;
            floor.body.CollidesWith = Category.All;
            floor.body.OnCollision += OnCollisionFloor;
            floor.body.CollisionCategories = Category.Cat11;
            boxes = new List<DrawablePhysicsObject>();
            //prevKeyboardState1 = Keyboard.GetState();

            slider = new DrawablePhysicsObject
            (
                world,
                Content.Load<Texture2D>("Paddle"),
                new Vector2(50, 16),
                10
            );
            slider.body.BodyType = BodyType.Dynamic;
            slider.Position = new Vector2(150, 400);
            slider.body.Rotation = 0;
            slider.body.CollidesWith = Category.All;
            slider.body.CollisionCategories = Category.Cat11;
            slider.body.CollisionGroup = 2;
            slider.body.OnCollision += Body_OnCollision;
            slider.body.Mass = 100;

            //Left and Right "Slider" limiters
            List<DrawablePhysicsObject> borders = new List<DrawablePhysicsObject>();
            var left = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Paddle"), new Vector2(5f, HEIGHT * 0.2f), 10000);
            left.Position = new Vector2(1, HEIGHT - 40);
            left.body.BodyType = BodyType.Static;
            left.body.CollidesWith = Category.All;
            left.body.CollisionCategories = Category.Cat11;
            var right = new DrawablePhysicsObject(world, Content.Load<Texture2D>("Paddle"), new Vector2(5f, HEIGHT * 0.2f), 10000);
            right.Position = new Vector2(WIDTH - 1, HEIGHT - 40);
            right.body.BodyType = BodyType.Static;
            right.body.CollidesWith = Category.All;
            right.body.CollisionCategories = Category.Cat11;



            paddles.Add(right);
            paddles.Add(left);
            paddles.Add(slider);

        }

        private bool OnCollisionFloor(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (fixtureB.CollisionGroup == 3)
            {
                lives -= 1;
                missSound.Play();
            }
            return true;
        }

        private bool Body_OnCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            if (fixtureB.CollisionCategories == Category.Cat2)
            {
                fixtureB.Body.ApplyAngularImpulse((float)(random.NextDouble() / 2 == 0 ? (random.NextDouble() + 1) * 1.5 : -(random.NextDouble() + 1) * 1.5));
                hitSound.Play();
                score++;
            }
            return true;
        }

        private void SpawnCrate()
        {
            boxes.Add(cube.SpawnCube(world, this));
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (isStarted)
            {
                currKeyboardState = Keyboard.GetState().IsKeyDown(Keys.Space);
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    if (prevKeyboardState != currKeyboardState)
                    {
                        isPaused = !isPaused;
                        if (isPaused)
                        {
                            soundEffectInstance.Pause();
                            pouseOn.Play();
                        }
                        else
                        {
                            pouseOff.Play();
                            soundEffectInstance.Play();
                        }
                    }
                }
                prevKeyboardState = Keyboard.GetState().IsKeyDown(Keys.Space);
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                    Exit();
                if (!isPaused)
                {
                    if (lives > 0)
                    {
                        float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
                        timer -= elapsed;

                        if (timer < 0)
                        {
                            //Timer expired, execute action
                            levelTimer -= levelTimer * 0.025f;
                            timer = levelTimer;   //Reset Timer
                            SpawnCrate();
                        }
                        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                            Exit();
                        if (Keyboard.GetState().IsKeyDown(Keys.Left))
                        {
                            if (slider.Position.X > 0.1)
                            {
                                slider.body.ApplyLinearImpulse(new Vector2(slider.body.LinearVelocity.X > 0 ? -100f : -35f, 0.001f));
                            }
                        }
                        if (Keyboard.GetState().IsKeyDown(Keys.Right))
                        {
                            slider.body.ApplyLinearImpulse(new Vector2(slider.body.LinearVelocity.X > 0 ? 100f : 35f, 0.001f));


                        }
                        world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
                        MouseState state = Mouse.GetState();
                        if (lives == 0)
                        {
                            loseSound.Play();

                        }
                    }
                    if (lives == 0 && Keyboard.GetState().IsKeyDown(Keys.R))
                    {
                        foreach (var item in boxes)
                        {
                            item.body.CollidesWith = Category.None;
                        }
                        boxes.Clear();
                        lives = 10;
                        timer = levelTimer = 3f;
                        score = 0;
                    }
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                prevKeyboardState = Keyboard.GetState().IsKeyDown(Keys.Space);
                isStarted = true;
            }
            currKeyboardState = Keyboard.GetState().IsKeyDown(Keys.Space);

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
            if (isStarted)
            {
                foreach (DrawablePhysicsObject crate in boxes)
                {
                    crate.Draw(spriteBatch);
                }
                foreach (DrawablePhysicsObject paddle in paddles)
                {
                    paddle.Draw(spriteBatch);
                }
                floor.Draw(spriteBatch);
                Vector2 textSize;
                if (lives < 1)
                {
                    spriteBatch.Draw(youLose, new Rectangle((int)(WIDTH - youLose.Width * 0.2) / 2, (int)(HEIGHT - youLose.Height * 0.2) / 2, (int)(youLose.Width * 0.2), (int)(youLose.Height * 0.2)), Color.White);
                    string text = "        Press 'R' \nPress 'ECS' to exit";
                    textSize = spriteFont.MeasureString(text);
                    spriteBatch.DrawString(spriteFont, text, new Vector2((WIDTH - textSize.X) / 2, (WIDTH - textSize.Y) / 2 + youLose.Width * 0.2f), Color.White);

                }
                string lives_s = "Lives: " + lives.ToString();
                string score_s = "Score: " + score.ToString();
                textSize = spriteFont.MeasureString(lives_s);
                spriteBatch.DrawString(spriteFont, lives_s, new Vector2(5, 5), Color.Maroon);
                spriteBatch.DrawString(spriteFont, score_s, new Vector2(5, textSize.Y + 5), Color.Khaki);
                if (isPaused)
                {
                    string pause_s = "PAUSE";
                    textSize = spriteFont.MeasureString(pause_s);
                    spriteBatch.DrawString(spriteFont, pause_s, new Vector2((WIDTH - textSize.X) / 2, (HEIGHT - textSize.Y) / 2), Color.Black);
                }
            }
            else
            {
                string welcome_s = "WELCOME TO SIDEWALK GUARD!\n      PRESS SPACE TO BEGIN!";
                Vector2 textSize = spriteFont.MeasureString(welcome_s);
                spriteBatch.DrawString(spriteFont, welcome_s, new Vector2((WIDTH - textSize.X) / 2 + (textSize.X / 2) * 0.35f, (HEIGHT - textSize.Y) / 2), Color.Black, 0.0f, new Vector2(0, 0), 0.65f, SpriteEffects.None, 0f);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
