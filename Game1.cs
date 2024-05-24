using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TDJ2_Astroidz
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Quit
    }
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private BasicEffect _basicEffect;
        private VertexPositionColor[] playerVertices;
        private Vector3 playerPosition = new Vector3(400, 300, 0);
        private Vector3 forwardDirection = new Vector3(0, 0, 0);
        private Vector3 perpendicularDirection = new Vector3(0, 0, 0);
        private Vector3 inertia = new Vector3(0, 0, 0);
        private float playerRotation = 0f;
        private float playerSpeed = 0.5f;
        public float PlayerHitPoints = 100f;
        private float playerFireTimer;
        private bool isPlayerAlive = true;

        private int asteroidTimer = 0;
        private List<VertexPositionColor[]> asteroidVertices = new List<VertexPositionColor[]>();
        private Random random = new Random();
        private List<Asteroid> asteroids = new List<Asteroid>();
        private const int AsteroidPoolSize = 50;

        public static List<Bullet> bullets = new List<Bullet>();

        private List<Enemy> enemies = new List<Enemy>();
        private int enemySpawnTimer = 0;
        private const int EnemySpawnInterval = 200;

        private Texture2D playerTexture;
        private Texture2D thrusterTexture;
        private Texture2D thrusterMidTexture;
        private Texture2D enemyTexture;
        private Texture2D projectileTexture;

        private Texture2D starTexture;
        private List<Vector2> stars;

        public GameState currentGameState = GameState.MainMenu;
        private Texture2D playButtonTexture;
        private Texture2D quitButtonTexture;
        private Rectangle playButtonRect;
        private Rectangle quitButtonRect;

        private float difficulty = 0f;
        private SpriteFont font;

        private SoundEffect bulletSoundEffect;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //Create the BasicEffect for drawing primitives
            _basicEffect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                Projection = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1)
            };

            //Load the font
            font = Content.Load<SpriteFont>("UIFont"); 

            //Load menu button textures
            playButtonTexture = Content.Load<Texture2D>("PlayButton");
            quitButtonTexture = Content.Load<Texture2D>("QuitButton");
            //Define menu button rectangles
            playButtonRect = new Rectangle(GraphicsDevice.Viewport.Width / 2 - playButtonTexture.Width / 2, GraphicsDevice.Viewport.Height / 2 - playButtonTexture.Height / 2, playButtonTexture.Width, playButtonTexture.Height);
            quitButtonRect = new Rectangle(GraphicsDevice.Viewport.Width / 2 - quitButtonTexture.Width / 2, GraphicsDevice.Viewport.Height / 2 + quitButtonTexture.Height, quitButtonTexture.Width, quitButtonTexture.Height);

            //Load game textures
            playerTexture = Content.Load<Texture2D>("Player01");
            thrusterTexture = Content.Load<Texture2D>("Thruster01");
            thrusterMidTexture = Content.Load<Texture2D>("Thruster02");
            enemyTexture = Content.Load<Texture2D>("Enemy01");
            projectileTexture = Content.Load<Texture2D>("projectile01");

            //Define player vertices for collisions
            playerVertices = new VertexPositionColor[3];
            playerVertices[0] = new VertexPositionColor(new Vector3(0, 20, 0), Color.White);
            playerVertices[1] = new VertexPositionColor(new Vector3(-10, -10, 0), Color.White);
            playerVertices[2] = new VertexPositionColor(new Vector3(10, -10, 0), Color.White);

            // Load star texture
            starTexture = Content.Load<Texture2D>("Star");
            // Generate random star positions
            GenerateStars();

            //Generate random asteroids
            Random random = new Random();
            for (int i = 0; i < AsteroidPoolSize; i++)
            {
                Vector2 position = new Vector2(random.Next(0, GraphicsDevice.Viewport.Width * 3), random.Next(0, GraphicsDevice.Viewport.Height * 3));
                Vector2 velocity = new Vector2((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1); //Random velocity
                asteroids.Add(CreateRandomAsteroid(position, velocity));
            }

            //Load Sound Effects
            bulletSoundEffect = Content.Load<SoundEffect>("plasmaShoot");

            //Load and play background music
            Song backgroundMusic = Content.Load<Song>("wrong-place-129242");
            MediaPlayer.Play(backgroundMusic);
            MediaPlayer.IsRepeating = true; //Loop the music

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (currentGameState)
            {
                case GameState.MainMenu:
                    UpdateMainMenu(gameTime);
                    break;
                case GameState.Playing:
                    UpdateGameplay(gameTime);
                    break;
                case GameState.Quit:
                    Exit();
                    break;
            }

            base.Update(gameTime);
        }

        private void UpdateMainMenu(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (playButtonRect.Contains(mouseState.Position))
                {
                    currentGameState = GameState.Playing;
                    difficulty = 0f;
                    PlayerHitPoints = 100f;
                    isPlayerAlive = true;
                }
                else if (quitButtonRect.Contains(mouseState.Position))
                {
                    currentGameState = GameState.Quit;
                }
            }
        }


        private void UpdateGameplay(GameTime gameTime)
        {

            if (isPlayerAlive)
            {
                if (PlayerHitPoints <= 0)
                {
                    isPlayerAlive = false;
                    PlayerHitPoints = 0;
                    currentGameState = GameState.MainMenu;
                }
            }

            //Update difficulty based on elapsed game time
            difficulty += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.1f;

            //Get current mouse state
            MouseState mouseState = Mouse.GetState();

            //Get current keyboard state
            KeyboardState keyboardState = Keyboard.GetState();

            //Calculate the screen diagonal
            float screenDiagonal = (float)Math.Sqrt(GraphicsDevice.Viewport.Width * GraphicsDevice.Viewport.Width + GraphicsDevice.Viewport.Height * GraphicsDevice.Viewport.Height);
            float inactiveThreshold = screenDiagonal * 3;

            //Reset inertia if no keys are pressed
            if (keyboardState.GetPressedKeys().Length == 0)
                inertia *= 0.95f; //Gradually decrease inertia magnitude

            //Speed vars prob want to move em later
            float forwardSpeedMultiplier = 1.0f;
            float strafeSpeedMultiplier = 0.5f;
            float backwardSpeedMultiplier = 0.2f;
            float maxSpeed = 20.0f;

            //Calculate movement based on current rotation
            forwardDirection = new Vector3((float)Math.Cos(playerRotation + MathHelper.PiOver2), (float)Math.Sin(playerRotation + MathHelper.PiOver2), 0);
            perpendicularDirection = new Vector3((float)Math.Cos(playerRotation), (float)Math.Sin(playerRotation), 0);

            if (keyboardState.IsKeyDown(Keys.W))
                inertia += forwardDirection * forwardSpeedMultiplier;
            if (keyboardState.IsKeyDown(Keys.S))
                inertia -= forwardDirection * backwardSpeedMultiplier;

            //Modify inertia calculation for strafing
            if (keyboardState.IsKeyDown(Keys.A))
                inertia += perpendicularDirection * strafeSpeedMultiplier;
            if (keyboardState.IsKeyDown(Keys.D))
                inertia -= perpendicularDirection * strafeSpeedMultiplier;
            //Handbrake
            if (keyboardState.IsKeyDown(Keys.Space))
                inertia *= 0.92f;

            //Cap the inertia to maximum speed
            if (inertia.LengthSquared() > maxSpeed * maxSpeed)
                inertia = Vector3.Normalize(inertia) * maxSpeed;

            playerPosition += inertia * playerSpeed;

            //Update player rotation to face mouse position
            forwardDirection = new Vector3(mouseState.X, mouseState.Y, 0) - new Vector3(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2, 0);
            playerRotation = (float)Math.Atan2(forwardDirection.Y, forwardDirection.X) - MathHelper.PiOver2;


            //Fire bullet on mouse click
            playerFireTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (playerFireTimer >= 0.1f)
            {
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    Vector2 bulletPosition = new Vector2(playerPosition.X, playerPosition.Y);
                    Vector2 bulletDirection = new Vector2(mouseState.X, mouseState.Y) - bulletPosition;
                    bullets.Add(new Bullet(bulletPosition, new Vector2(forwardDirection.X, forwardDirection.Y), 1));
                    playerFireTimer = 0f;

                    bulletSoundEffect.Play();
                }
            }

            //Generate new asteroids from the pool
            if (asteroidTimer >= 10)
            {
                //Find an inactive asteroid from the pool
                Asteroid newAsteroid = asteroids.FirstOrDefault(a => !a.IsActive);
                if (newAsteroid != null)
                {
                    //Reset the asteroid's properties and activate it
                    Vector2 position = new Vector2(random.Next(-GraphicsDevice.Viewport.Width * 2 + (int)playerPosition.X, GraphicsDevice.Viewport.Width * 2 + (int)playerPosition.X), random.Next(-GraphicsDevice.Viewport.Height * 2 + (int)playerPosition.Y, GraphicsDevice.Viewport.Height * 2 + (int)playerPosition.Y));
                    Vector2 velocity = new Vector2((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1); //Random velocity
                    newAsteroid.Reset(position, velocity);
                    newAsteroid.IsActive = true;
                }
                asteroidTimer = 0;
            }
            else {asteroidTimer++;}


            Matrix playerTransform = Matrix.CreateRotationZ(playerRotation) * Matrix.CreateTranslation(playerPosition);

            //Asteroid collision and updating logic
            foreach (var asteroid in asteroids)
            {
                if (!asteroid.IsActive) continue;  //Skip inactive asteroids
                asteroid.Update(playerPosition, inactiveThreshold);

                //Player-Asteroid Collision Detection
                asteroid.CheckCollisionPlayer(playerVertices, playerTransform, ref inertia, playerSpeed, 1.0f, ref PlayerHitPoints, playerPosition);

                //Asteroid-Enemy Collision Detection
                foreach (var enemy in enemies)
                {
                    asteroid.CheckCollisionEnemy(enemy);
                }

            }
            //Asteroid-Asteroid Collision Detection
            for (int i = 0; i < asteroids.Count; i++)
            {
                for (int j = i + 1; j < asteroids.Count; j++)
                {
                    asteroids[i].CheckCollisionOtherAsteroids(asteroids[j]);
                }
            }

            //Remove inactive bullets
            bullets.RemoveAll(b => !b.IsActive);
            //Update Bullets
            foreach (var bullet in bullets)
            {
                bullet.Update(playerPosition, inactiveThreshold);
                bullet.HandleCollisions(asteroids, enemies, playerVertices, playerTransform, ref PlayerHitPoints);
            }

            //Spawn enemies at intervals
            enemySpawnTimer++;
            if (enemySpawnTimer >= EnemySpawnInterval)
            {
                SpawnEnemy();
                enemySpawnTimer = 0;
            }
            //Update enemies
            foreach (var enemy in enemies)
            {
                enemy.Update(new Vector2(playerPosition.X, playerPosition.Y), gameTime, playerVertices, playerTransform, ref inertia, playerSpeed, ref PlayerHitPoints, asteroids);
            }
            //Remove inactive enemies
            enemies.RemoveAll(e => !e.IsActive);
        }



        private Asteroid CreateRandomAsteroid(Vector2 position, Vector2 velocity)
        {
            int numVertices = random.Next(5, 10);
            List<VertexPositionColor> asteroidVertices = new List<VertexPositionColor>();

            float semiMajorAxis = 50.0f;
            float semiMinorAxis = 30.0f;

            Vector3 center = Vector3.Zero;

            //Generate vertices around the center
            for (int i = 0; i < numVertices; i++)
            {
                float angle = (float)i / numVertices * MathHelper.TwoPi;
                float x = semiMajorAxis * (float)Math.Cos(angle);
                float y = semiMinorAxis * (float)Math.Sin(angle);
                asteroidVertices.Add(new VertexPositionColor(new Vector3(x, y, 0), Color.DarkOrange));
            }

            //Create triangles
            List<VertexPositionColor> triangleList = new List<VertexPositionColor>();
            for (int i = 0; i < numVertices; i++)
            {
                int nextIndex = (i + 1) % numVertices;

                //Triangle from center to current vertex to next vertex
                triangleList.Add(new VertexPositionColor(center, Color.White));
                triangleList.Add(asteroidVertices[i]);
                triangleList.Add(asteroidVertices[nextIndex]);
            }

            return new Asteroid(triangleList.ToArray(), position, velocity);
        }

        private void SpawnEnemy()
        {
            //This is a hacky fix to prevent crashes
            //Calculate the boundaries of the spawn area ensuring they are within the viewport dimensions
            int xMin = Math.Max(0, (int)playerPosition.X - GraphicsDevice.Viewport.Width / 2);
            int xMax = Math.Min(GraphicsDevice.Viewport.Width, (int)playerPosition.X + GraphicsDevice.Viewport.Width / 2);
            xMax = Math.Max(xMin + 1, xMax);  // Ensure xMax is greater than xMin

            int yMin = Math.Max(0, (int)playerPosition.Y - GraphicsDevice.Viewport.Height / 2);
            int yMax = Math.Min(GraphicsDevice.Viewport.Height, (int)playerPosition.Y + GraphicsDevice.Viewport.Height / 2);
            yMax = Math.Max(yMin + 1, yMax);  // Ensure yMax is greater than yMin

            //Generate a random position within the defined boundaries
            Vector2 position = new Vector2(random.Next(xMin, xMax), random.Next(yMin, yMax));

            //Choose where to place the enemy (This code causes crashes, because of min/max values of the randoms).
            //Vector2 position = new Vector2(random.Next(0, GraphicsDevice.Viewport.Width), random.Next(0, GraphicsDevice.Viewport.Height);
            //Vector2 position = new Vector2(random.Next(0, GraphicsDevice.Viewport.Width + (int)playerPosition.X), random.Next(0, GraphicsDevice.Viewport.Height + (int)playerPosition.Y));

            //List of possible enemy types, needs to be manually updated everytime a new enemy is added to Enemy.cs
            int[] enemyTypes = new int[] { 1, 2, 3 };

            //Create a list to store adjusted spawn weights
            List<float> adjustedSpawnWeights = new List<float>();

            //For each enemy type, a temporary Enemy object is "created" with the given position and type, for ease of use, doesn't actually enter gameplay at all.
            foreach (int type in enemyTypes)
            {
                Enemy tempEnemy = new Enemy(position, playerVertices, type);
                adjustedSpawnWeights.Add(tempEnemy.AdjustedSpawnWeight(difficulty));
            }

            //Calculate the total weight
            float totalWeight = adjustedSpawnWeights.Sum();

            //Pick a random value within the total weight range
            float randomValue = (float)(random.NextDouble() * totalWeight);

            //Determine which enemy type to spawn based on the random value
            float cumulativeWeight = 0.0f;
            int chosenType = 1;
            for (int i = 0; i < enemyTypes.Length; i++)
            {
                cumulativeWeight += adjustedSpawnWeights[i];
                if (randomValue < cumulativeWeight)
                {
                    chosenType = enemyTypes[i];
                    break;
                }
            }

            enemies.Add(new Enemy(position, playerVertices, chosenType));
        }

        //Handles calling the draw for either main menu or playing
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            switch (currentGameState)
            {
                case GameState.MainMenu:
                    DrawMainMenu();
                    break;
                case GameState.Playing:
                    DrawGameplay(gameTime);
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawMainMenu()
        {
            DrawStars(_spriteBatch);

            _spriteBatch.Draw(playButtonTexture, playButtonRect, Color.White);
            _spriteBatch.Draw(quitButtonTexture, quitButtonRect, Color.White);
        }

        private void DrawGameplay(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            DrawStars(_spriteBatch);

            //Set the player's position to the center of the screen
            Vector3 screenCenter = new Vector3(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2, 0);

            //Draw the player sprite
            Vector2 playerOrigin = new Vector2(playerTexture.Width / 2, playerTexture.Height / 2);
            _spriteBatch.Draw(playerTexture, new Vector2(screenCenter.X, screenCenter.Y), null, Color.White, playerRotation + MathHelper.PiOver2*2, playerOrigin, 1.0f, SpriteEffects.None, 0f);


            //If the player is moving forward, draw the thrusters
            if (Vector3.Dot(inertia, forwardDirection) > 150)
            {
                _spriteBatch.Draw(thrusterTexture, new Vector2(screenCenter.X, screenCenter.Y), null, Color.White, playerRotation + MathHelper.PiOver2 * 2, new Vector2(playerOrigin.X - 3, playerOrigin.Y - 40), 1.0f, SpriteEffects.None, 0f);
                _spriteBatch.Draw(thrusterMidTexture, new Vector2(screenCenter.X, screenCenter.Y), null, Color.White, playerRotation + MathHelper.PiOver2*2, new Vector2(playerOrigin.X-19, playerOrigin.Y - 40), 1.0f, SpriteEffects.None, 0f);
                _spriteBatch.Draw(thrusterTexture, new Vector2(screenCenter.X, screenCenter.Y), null, Color.White, playerRotation + MathHelper.PiOver2 * 2, new Vector2(playerOrigin.X -35, playerOrigin.Y - 40), 1.0f, SpriteEffects.None, 0f);
            }

            //Adjust the view matrix for the asteroids and other objects
            Matrix cameraTranslation = Matrix.CreateTranslation(screenCenter - playerPosition);

            foreach (var asteroid in asteroids)
            {
                Matrix asteroidWorldMatrix = Matrix.CreateRotationZ(asteroid.rotation) * Matrix.CreateTranslation(new Vector3(asteroid.position, 0));

                _basicEffect.World = asteroidWorldMatrix;
                _basicEffect.View = cameraTranslation;
                _basicEffect.CurrentTechnique.Passes[0].Apply();

                asteroid.Draw(GraphicsDevice, _basicEffect);

                //Debug.DrawLine(GraphicsDevice, Vector3.Transform(new Vector3(asteroid.position.X, asteroid.position.Y, 0), cameraTranslation), Vector3.Transform(new Vector3(asteroid.position.X, asteroid.position.Y, 0) + new Vector3(asteroid.velocity.X, asteroid.velocity.Y, 0) * 100, cameraTranslation), Color.Blue);
            }

            foreach (var enemy in enemies)
            {
                //Transform enemy position to screen space
                Vector3 enemyScreenPosition = Vector3.Transform(new Vector3(enemy.Position, 0), cameraTranslation);

                //Draw the enemy texture at the transformed position with the correct rotation
                Vector2 enemyOrigin = new Vector2(enemyTexture.Width / 2, enemyTexture.Height / 2);
                _spriteBatch.Draw(enemyTexture, new Vector2(enemyScreenPosition.X, enemyScreenPosition.Y), null, Color.White, enemy.Rotation, enemyOrigin, 1.0f, SpriteEffects.None, 0f);

                //Debug.DrawLine(GraphicsDevice, enemyScreenPosition, enemyScreenPosition + new Vector3(enemy.Velocity.X, enemy.Velocity.Y, 0), Color.Purple);
            }

            //Draw bullets
            foreach (var bullet in bullets)
            {
                //bullet.Draw(GraphicsDevice, _basicEffect, cameraTranslation); Not used atm

                //Transform bullet position to screen space
                Vector3 bulletScreenPosition = Vector3.Transform(new Vector3(bullet.Position, 0), cameraTranslation);

                //Draw the bullet texture at the transformed position with the correct rotation
                Vector2 bulletOrigin = new Vector2(projectileTexture.Width / 2, projectileTexture.Height / 2);
                _spriteBatch.Draw(projectileTexture, new Vector2(bulletScreenPosition.X, bulletScreenPosition.Y), null, Color.White, bullet.Rotation, bulletOrigin, 1.0f, SpriteEffects.None, 0f);

                //Debug.DrawLine(GraphicsDevice, Vector3.Transform(new Vector3(bullet.Position.X, bullet.Position.Y, 0), cameraTranslation), Vector3.Transform(new Vector3(bullet.Position.X, bullet.Position.Y, 0) + new Vector3(bullet.Velocity.X, bullet.Velocity.Y, 0) * -1, cameraTranslation), Color.Red);
            }

            //Draw the UI
            _spriteBatch.DrawString(font, $"Health: {(int)PlayerHitPoints}", new Vector2(10, 10), Color.Green);
            _spriteBatch.DrawString(font, $"Difficulty: {(int)difficulty}", new Vector2(10, GraphicsDevice.Viewport.Height-20), Color.Yellow);
        }

        private void GenerateStars()
        {
            stars = new List<Vector2>();

            // Generate random star positions
            Random random = new Random();
            int numStars = 500; // Adjust as needed
            for (int i = 0; i < numStars; i++)
            {
                int x = random.Next(0, GraphicsDevice.Viewport.Width);
                int y = random.Next(0, GraphicsDevice.Viewport.Height);
                stars.Add(new Vector2(x, y));
            }
        }

        private void DrawStars(SpriteBatch spriteBatch)
        {
            // Draw stars
            foreach (Vector2 star in stars)
            {
                spriteBatch.Draw(starTexture, star, Color.White);
            }
        }


    }

    public static class Debug
    {
        public static void DrawLine(GraphicsDevice graphicsDevice, Vector3 start, Vector3 end, Color color)
        {
            VertexPositionColor[] vertices = new VertexPositionColor[2];
            vertices[0] = new VertexPositionColor(start, color);
            vertices[1] = new VertexPositionColor(end, color);

            BasicEffect basicEffect = new BasicEffect(graphicsDevice)
            {
                VertexColorEnabled = true,
                View = Matrix.Identity,
                Projection = Matrix.CreateOrthographicOffCenter(0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0, 0, 1)
            };

            basicEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }
    }


    //SAT collision
    public static class SATCollision
    {
        //Takes in two arrays of vertices(shapeA and shapeB) and their corresponding transformation matrices(transformA and transformB)
        //Calls GetAxes method to obtain the axes of both shapes
        //Iterates through each axis and checks for overlap using the IsOverlapping method
        //If any axis shows no overlap, it returns false, indicating no collision.Otherwise, it returns true
        public static bool CheckCollision(VertexPositionColor[] shapeA, Matrix transformA, VertexPositionColor[] shapeB, Matrix transformB)
        {
            List<Vector2> axes = GetAxes(shapeA, transformA);
            axes.AddRange(GetAxes(shapeB, transformB));

            foreach (var axis in axes)
            {
                if (!IsOverlapping(axis, shapeA, transformA, shapeB, transformB))
                {
                    return false;
                }
            }

            return true;
        }

        //Calculates the axes of a shape by iterating through its vertices
        //For each edge defined by two consecutive vertices, it calculates the normal vector(perpendicular to the edge) and adds it to the list of axes
        //It then returns the list of axes
        private static List<Vector2> GetAxes(VertexPositionColor[] shape, Matrix transform)
        {
            List<Vector2> axes = new List<Vector2>();

            for (int i = 0; i < shape.Length; i++)
            {
                Vector2 p1 = Vector2.Transform(new Vector2(shape[i].Position.X, shape[i].Position.Y), transform);
                Vector2 p2 = Vector2.Transform(new Vector2(shape[(i + 1) % shape.Length].Position.X, shape[(i + 1) % shape.Length].Position.Y), transform);

                Vector2 edge = p2 - p1;
                Vector2 normal = new Vector2(-edge.Y, edge.X);
                normal.Normalize();

                axes.Add(normal);
            }

            return axes;
        }

        //Checks if two shapes are overlapping along a given axis
        //Projects both shapes onto the axis using the ProjectShape method
        //Compares the projections to determine if there is overlap
        //Returns true if there is overlap, false otherwise
        private static bool IsOverlapping(Vector2 axis, VertexPositionColor[] shapeA, Matrix transformA, VertexPositionColor[] shapeB, Matrix transformB)
        {
            var (minA, maxA) = ProjectShape(axis, shapeA, transformA);
            var (minB, maxB) = ProjectShape(axis, shapeB, transformB);

            return minA <= maxB && maxA >= minB;
        }

        //Projects a shape onto a given axis
        //Iterates through each vertex of the shape, transforms it using the provided transformation matrix, and calculates its projection onto the axis using dot product
        //Keeps track of the minimum and maximum projections
        //Returns a tuple containing the minimum and maximum projections
        private static (float min, float max) ProjectShape(Vector2 axis, VertexPositionColor[] shape, Matrix transform)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            foreach (var vertex in shape)
            {
                Vector2 transformedVertex = Vector2.Transform(new Vector2(vertex.Position.X, vertex.Position.Y), transform);
                float projection = Vector2.Dot(transformedVertex, axis);

                if (projection < min) min = projection;
                if (projection > max) max = projection;
            }

            return (min, max);
        }
    }


}




