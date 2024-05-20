using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TDJ2_Astroidz
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private BasicEffect _basicEffect;
        private VertexPositionColor[] playerVertices;
        private Vector3 playerPosition = new Vector3(400, 300, 0); //Initial position
        private Vector3 forwardDirection = new Vector3(0, 0, 0);
        private Vector3 perpendicularDirection = new Vector3(0, 0, 0);
        private Vector3 inertia = new Vector3(0, 0, 0);
        private float playerRotation = 0f;
        private float playerSpeed = 0.5f;

        private int asteroidTimer = 0;
        private List<VertexPositionColor[]> asteroidVertices = new List<VertexPositionColor[]>();
        private Random random = new Random();
        private List<Asteroid> asteroids = new List<Asteroid>();
        private const int AsteroidPoolSize = 50;

        public float PlayerHitPoints = 100f;
        private float playerFireTimer;

        public static List<Bullet> bullets = new List<Bullet>();

        private List<Enemy> enemies = new List<Enemy>();
        private int enemySpawnTimer = 0;
        private const int EnemySpawnInterval = 200;

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

            //Define player vertices to draw a triangle
            playerVertices = new VertexPositionColor[3];
            playerVertices[0] = new VertexPositionColor(new Vector3(0, 20, 0), Color.White);
            playerVertices[1] = new VertexPositionColor(new Vector3(-10, -10, 0), Color.White);
            playerVertices[2] = new VertexPositionColor(new Vector3(10, -10, 0), Color.White);

            //Generate random asteroids
            Random random = new Random();
            for (int i = 0; i < AsteroidPoolSize; i++)
            {
                Vector2 position = new Vector2(random.Next(0, GraphicsDevice.Viewport.Width * 3), random.Next(0, GraphicsDevice.Viewport.Height * 3));
                Vector2 velocity = new Vector2((float)random.NextDouble() * 2 - 1, (float)random.NextDouble() * 2 - 1); //Random velocity
                asteroids.Add(CreateRandomAsteroid(position, velocity));
            }

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

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
            float strafeSpeedMultiplier = 0.33f;
            float backwardSpeedMultiplier = 0.15f;
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
            else
            {
                asteroidTimer++;
            }

            Matrix playerTransform = Matrix.CreateRotationZ(playerRotation) * Matrix.CreateTranslation(playerPosition);

            foreach (var asteroid in asteroids)
            {
                if (!asteroid.IsActive) continue;  //Skip inactive asteroids
                asteroid.Update();

                //Calculate distance from player to asteroid
                float distanceToPlayer = Vector2.Distance(new Vector2(playerPosition.X, playerPosition.Y), asteroid.position);
                //Set asteroid as inactive if beyond the threshold distance
                if (distanceToPlayer > inactiveThreshold)
                {
                    asteroid.IsActive = false;
                }

                //Player-Asteroid Collision Detection
                Matrix asteroidTransform = Matrix.CreateRotationZ(asteroid.rotation) * Matrix.CreateTranslation(new Vector3(asteroid.position, 0));

                if (SATCollision.CheckCollision(playerVertices, playerTransform, asteroid.vertices, asteroidTransform))
                {
                    Vector2 collisionNormal = Vector2.Normalize(new Vector2(playerPosition.X, playerPosition.Y) - asteroid.position);

                    //Reflect the player's inertia
                    Vector3 playerVelocity = inertia * playerSpeed;
                    Vector3 relativeVelocity = playerVelocity - new Vector3(asteroid.velocity, 0);

                    //Calculate the impact speed along the collision normal
                    float impactSpeed = Vector3.Dot(relativeVelocity, new Vector3(collisionNormal, 0));

                    if (impactSpeed > 0)
                        continue; //Objects are separating, no need to adjust velocities

                    //Calculate new velocities based on conservation of momentum
                    float playerMass = 1.0f; //Assume player mass is 1 for simplicity
                    float totalMass = playerMass + asteroid.Mass;
                    float impulse = 2 * impactSpeed / totalMass;

                    Vector3 impulseVector = impulse * asteroid.Mass * new Vector3(collisionNormal, 0);
                    inertia -= impulseVector / playerSpeed;
                    inertia *= 0.9f; //Dampen the inertia slightly to simulate energy loss

                    //Ensure a minimum inertia to prevent stopping completely
                    if (inertia.Length() < 0.1f)
                    {
                        inertia = new Vector3(collisionNormal * -0.5f, 0);
                    }

                    asteroid.HandleCollision(collisionNormal, playerSpeed, playerMass);


                    //Calculate the impact force (use the magnitude of the impulse vector)
                    float impactForce = impulseVector.Length();
                    //Reduce player's health based on the impact force
                    float healthReduction = impactForce / 3;
                    PlayerHitPoints -= healthReduction;
                    if (PlayerHitPoints < 0) PlayerHitPoints = 0; //Ensure health doesn't go below 0
                    Console.WriteLine(PlayerHitPoints.ToString());
                }
            }

            //Asteroid-Asteroid Collision Detection
            for (int i = 0; i < asteroids.Count; i++)
            {
                for (int j = i + 1; j < asteroids.Count; j++)
                {
                    Asteroid asteroidA = asteroids[i];
                    Asteroid asteroidB = asteroids[j];

                    if (!asteroidA.IsActive || !asteroidB.IsActive)
                        continue;

                    Matrix transformA = Matrix.CreateRotationZ(asteroidA.rotation) * Matrix.CreateTranslation(new Vector3(asteroidA.position, 0));
                    Matrix transformB = Matrix.CreateRotationZ(asteroidB.rotation) * Matrix.CreateTranslation(new Vector3(asteroidB.position, 0));

                    if (SATCollision.CheckCollision(asteroidA.vertices, transformA, asteroidB.vertices, transformB))
                    {
                        Vector2 collisionNormal = Vector2.Normalize(asteroidA.position - asteroidB.position);

                        //Calculate relative velocity
                        Vector2 relativeVelocity = asteroidA.velocity - asteroidB.velocity;

                        //Calculate the impact speed along the collision normal
                        float impactSpeed = Vector2.Dot(relativeVelocity, collisionNormal);

                        if (impactSpeed > 0)
                            continue; //Objects are separating, no need to adjust velocities

                        //Calculate new velocities based on conservation of momentum
                        float totalMass = asteroidA.Mass + asteroidB.Mass;
                        float impulse = 2 * impactSpeed / totalMass;

                        asteroidA.velocity -= impulse * asteroidB.Mass * collisionNormal;
                        asteroidB.velocity += impulse * asteroidA.Mass * collisionNormal;

                        //Dampen the velocities slightly to simulate energy loss
                        asteroidA.velocity *= 0.9f;
                        asteroidB.velocity *= 0.9f;

                        //Ensure a minimum velocity to prevent stopping completely
                        if (asteroidA.velocity.Length() < 0.1f)
                        {
                            asteroidA.velocity = collisionNormal * -0.5f;
                        }
                        if (asteroidB.velocity.Length() < 0.1f)
                        {
                            asteroidB.velocity = collisionNormal * 0.5f;
                        }
                    }
                }
            }

            //Remove inactive bullets
            bullets.RemoveAll(b => !b.IsActive);

            foreach (var bullet in bullets)
            {
                bullet.Update(playerPosition, inactiveThreshold);

                if (bullet.Faction != 1) //Not Player Faction
                {
                    if (SATCollision.CheckCollision(new VertexPositionColor[] {  new VertexPositionColor(new Vector3(bullet.Position, 0), Color.White),  new VertexPositionColor(new Vector3(bullet.Position + new Vector2(1, 0), 0), Color.White) }, Matrix.Identity, playerVertices, playerTransform))
                    {
                        bullet.IsActive = false;
                        PlayerHitPoints -= 10f;
                        if (PlayerHitPoints < 0) PlayerHitPoints = 0;
                        Console.WriteLine(PlayerHitPoints.ToString());
                    }
                }

                //Check collision with asteroids
                foreach (var asteroid in asteroids)
                {
                    if (!asteroid.IsActive) continue;

                    Matrix asteroidTransform = Matrix.CreateRotationZ(asteroid.rotation) * Matrix.CreateTranslation(new Vector3(asteroid.position, 0));
                    if (SATCollision.CheckCollision(new VertexPositionColor[] {
                    new VertexPositionColor(new Vector3(bullet.Position, 0), Color.White),
                    new VertexPositionColor(new Vector3(bullet.Position + new Vector2(1, 0), 0), Color.White)
                }, Matrix.Identity, asteroid.vertices, asteroidTransform))
                        {
                            bullet.IsActive = false;
                            asteroid.Hitpoints -= 20f;
                            if (asteroid.Hitpoints <= 0)
                            {
                                asteroid.IsActive = false;
                            }
                            break; //No need to check other asteroids if a collision is detected
                        }
                    }
            
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
                enemy.Update(new Vector2(playerPosition.X, playerPosition.Y), gameTime);

                //Check for collisions between player and enemies

                if (enemy.CheckCollision(playerVertices, playerTransform))
                {
                    //Handle player-enemy collision
                    Vector2 collisionNormal = Vector2.Normalize(new Vector2(playerPosition.X, playerPosition.Y) - enemy.Position);
                    Vector3 playerVelocity = inertia * playerSpeed;
                    Vector3 relativeVelocity = playerVelocity - new Vector3(enemy.Velocity, 0);

                    float impactSpeed = Vector3.Dot(relativeVelocity, new Vector3(collisionNormal, 0));
                    if (impactSpeed > 0)
                        continue;

                    float playerMass = 1.0f;
                    float totalMass = playerMass + 1.0f; //Assuming enemy mass is also 1 for simplicity
                    float impulse = 2 * impactSpeed / totalMass;
                    Vector3 impulseVector = impulse * 1.0f * new Vector3(collisionNormal, 0);
                    inertia -= impulseVector / playerSpeed;
                    inertia *= 0.9f;

                    if (inertia.Length() < 0.1f)
                    {
                        inertia = new Vector3(collisionNormal * -0.5f, 0);
                    }

                    //Reduce player's health based on the impact force
                    float impactForce = impulseVector.Length();
                    float healthReduction = impactForce / 10;
                    PlayerHitPoints -= healthReduction;
                    if (PlayerHitPoints < 0) PlayerHitPoints = 0;
                    Console.WriteLine(PlayerHitPoints.ToString());

                    //Handle enemy damage if necessary
                    enemy.HitPoints -= healthReduction;
                    if (enemy.HitPoints <= 0) enemy.IsActive = false;
                }
            }

            //Remove inactive enemies
            enemies.RemoveAll(e => !e.IsActive);

            base.Update(gameTime);
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
                asteroidVertices.Add(new VertexPositionColor(new Vector3(x, y, 0), Color.White));
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
            Vector2 position = new Vector2(random.Next(0, GraphicsDevice.Viewport.Width), random.Next(0, GraphicsDevice.Viewport.Height));
            float speed = 300f;
            float fireRate = 0.2f;
            float hitPoints = 100f;
            enemies.Add(new Enemy(position, speed, fireRate, hitPoints, playerVertices)); //Pass playerVertices
        }




        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            //Set the player's position to the center of the screen
            Vector3 screenCenter = new Vector3(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2, 0);

            //Transform player vertices with rotation
            _basicEffect.World = Matrix.CreateRotationZ(playerRotation) * Matrix.CreateTranslation(screenCenter);
            _basicEffect.View = Matrix.Identity;
            _basicEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, playerVertices, 0, 1);

            //Adjust the view matrix for the asteroids and other objects
            Matrix cameraTranslation = Matrix.CreateTranslation(screenCenter - playerPosition);

            foreach (var asteroid in asteroids)
            {
                Matrix asteroidWorldMatrix = Matrix.CreateRotationZ(asteroid.rotation) * Matrix.CreateTranslation(new Vector3(asteroid.position, 0));

                _basicEffect.World = asteroidWorldMatrix;
                _basicEffect.View = cameraTranslation;
                _basicEffect.CurrentTechnique.Passes[0].Apply();

                asteroid.Draw(GraphicsDevice, _basicEffect);

                Debug.DrawLine(GraphicsDevice,
                               Vector3.Transform(new Vector3(asteroid.position.X, asteroid.position.Y, 0), cameraTranslation),
                               Vector3.Transform(new Vector3(asteroid.position.X, asteroid.position.Y, 0) + new Vector3(asteroid.velocity.X, asteroid.velocity.Y, 0) * 100, cameraTranslation),
                               Color.Blue);
            }

            foreach (var enemy in enemies)
            {
                Matrix asteroidWorldMatrix = Matrix.CreateRotationZ(enemy.Rotation) * Matrix.CreateTranslation(new Vector3(enemy.Position, 0));

                _basicEffect.World = asteroidWorldMatrix;
                _basicEffect.View = cameraTranslation;
                _basicEffect.CurrentTechnique.Passes[0].Apply();

                Debug.DrawLine(GraphicsDevice,
                               Vector3.Transform(new Vector3(enemy.Position.X, enemy.Position.Y, 0), cameraTranslation),
                               Vector3.Transform(new Vector3(enemy.Position.X, enemy.Position.Y, 0) + new Vector3(enemy.Velocity.X, enemy.Velocity.Y, 0), cameraTranslation),
                               Color.Purple);
            }

            //Draw bullets
            foreach (var bullet in bullets)
            {
                bullet.Draw(GraphicsDevice, _basicEffect, cameraTranslation);
                Debug.DrawLine(GraphicsDevice,
                               Vector3.Transform(new Vector3(bullet.Position.X, bullet.Position.Y, 0), cameraTranslation),
                               Vector3.Transform(new Vector3(bullet.Position.X, bullet.Position.Y, 0) + new Vector3(bullet.Velocity.X, bullet.Velocity.Y, 0) * 1, cameraTranslation),
                               Color.Red);
            }

            base.Draw(gameTime);
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



    public static class SATCollision
    {
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

        private static bool IsOverlapping(Vector2 axis, VertexPositionColor[] shapeA, Matrix transformA, VertexPositionColor[] shapeB, Matrix transformB)
        {
            var (minA, maxA) = ProjectShape(axis, shapeA, transformA);
            var (minB, maxB) = ProjectShape(axis, shapeB, transformB);

            return minA <= maxB && maxA >= minB;
        }

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




