using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace TDJ2_Astroidz
{
    public class Enemy
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public bool IsActive;
        public float FireRate;
        private float fireTimer;
        public float Speed;
        public float HitPoints;
        public float Mass;
        public VertexPositionColor[] Vertices;
        public Texture2D Texture;

        public int Faction = 2;
        public int Type;
        public float spawnWeight;

        public Enemy(Vector2 position, VertexPositionColor[] vertices, int enemyType)
        {
            //Default setup
            Type = enemyType;
            Position = position;
            IsActive = true;
            fireTimer = 0f; //This is not fireRate, this is just a timer for firing
            Vertices = vertices;
            HitPoints = 50;
            Speed = 300;
            FireRate = 0.33f;
            Mass = 0.01f;

            //Here we initialize different enemy types and stats etc
            switch (Type)
            {
                case 1:
                    HitPoints = 50;
                    Speed = 300;
                    FireRate = 0.4f;
                    spawnWeight = 100f;
                    break;
                case 2:
                    HitPoints = 100;
                    Speed = 150;
                    FireRate = 0.25f;
                    spawnWeight = 30f;
                    break;
                case 3:
                    HitPoints = 50;
                    Speed = 550;
                    FireRate = 0.7f;
                    spawnWeight = 10f;
                    break;
            }
        }

        public float AdjustedSpawnWeight(float difficulty)
        {
            //Adjust spawn weight based on difficulty, making rarer enemies more common as difficulty increases
            return spawnWeight / (difficulty / 100.0f + 1.0f);
        }

        public void Update(Vector2 playerPosition, GameTime gameTime, VertexPositionColor[] playerVertices, Matrix playerTransform, ref Vector3 inertia, float playerSpeed, ref float playerHitPoints, List<Asteroid> asteroids)
        {
            if (!IsActive)
                return;

            //Calculate direction towards player
            Vector2 direction = playerPosition - Position;
            direction.Normalize();

            //Perform raycasting to detect asteroids
            Asteroid hitAsteroid;
            float rayLength = 100f;
            if (Raycast(Position, direction, rayLength, asteroids, out hitAsteroid))
            {
                //Avoidance maneuver
                Vector2 avoidanceDirection = Vector2.Transform(direction, Matrix.CreateRotationZ(MathHelper.PiOver2));
                direction = (avoidanceDirection * 7.5f + direction * 0.5f);
                direction.Normalize();
            }

            //Move towards player
            Velocity = direction * Speed;
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Update rotation to face the movement direction
            if (Velocity.LengthSquared() > 0)
            {
                Rotation = (float)Math.Atan2(Velocity.Y, Velocity.X) + MathHelper.PiOver2;
            }

            //Update firing logic
            fireTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (fireTimer >= FireRate)
            {
                FireBullet(direction);
                fireTimer = 0f;
            }

            //Check and handle collision with player
            HandlePlayerCollision(playerVertices, playerTransform, ref inertia, playerSpeed, ref playerHitPoints, new Vector3(playerPosition, 0));
        }

        private void FireBullet(Vector2 direction)
        {
            //Add a new bullet fired towards the player
            Game1.bullets.Add(new Bullet(Position, direction, 2));
        }

        public void HandlePlayerCollision(VertexPositionColor[] playerVertices, Matrix playerTransform, ref Vector3 inertia, float playerSpeed, ref float playerHitPoints, Vector3 playerPosition)
        {
            if (CheckCollision(playerVertices, playerTransform))
            {
                //Handle player-enemy collision
                Vector2 collisionNormal = Vector2.Normalize(new Vector2(playerPosition.X, playerPosition.Y) - Position);
                Vector3 playerVelocity = inertia * playerSpeed;
                Vector3 relativeVelocity = playerVelocity - new Vector3(Velocity, 0);

                float impactSpeed = Vector3.Dot(relativeVelocity, new Vector3(collisionNormal, 0));
                if (impactSpeed > 0)
                    return;

                float playerMass = 1.0f;
                float enemyMass = 1.0f; //Assuming enemy mass is also 1 for simplicity
                float totalMass = playerMass + enemyMass;
                float impulse = 2 * impactSpeed / totalMass;
                Vector3 impulseVector = impulse * enemyMass * new Vector3(collisionNormal, 0);
                inertia -= impulseVector / playerSpeed;
                inertia *= 0.9f;

                if (inertia.Length() < 0.1f)
                {
                    inertia = new Vector3(collisionNormal * -0.5f, 0);
                }

                //Reduce player's health based on the impact force
                float impactForce = impulseVector.Length();
                float healthReduction = impactForce / 1000;
                playerHitPoints -= healthReduction;
                if (playerHitPoints < 0) playerHitPoints = 0;
                Console.WriteLine(playerHitPoints.ToString());

                //Handle enemy damage if necessary
                HitPoints -= healthReduction * 1000;
                if (HitPoints <= 0) IsActive = false;
            }
        }

        public bool CheckCollision(VertexPositionColor[] otherVertices, Matrix otherTransform)
        {
            Matrix enemyTransform = Matrix.CreateRotationZ(Rotation) * Matrix.CreateTranslation(new Vector3(Position, 0));
            return SATCollision.CheckCollision(Vertices, enemyTransform, otherVertices, otherTransform);
        }

        public bool Raycast(Vector2 origin, Vector2 direction, float length, List<Asteroid> asteroids, out Asteroid hitAsteroid)
        {
            hitAsteroid = null;
            float closestDistance = float.MaxValue;

            foreach (var asteroid in asteroids)
            {
                if (!asteroid.IsActive) continue;

                //Check if the ray intersects the current asteroid being iterated over
                if (RayIntersectsAsteroid(origin, direction, length, asteroid, out float distance))
                {
                    //If the current intersection distance is closer than the previous closest distance, update it
                    //Supposed to help prevent it from crashing into asteroids but uh. Not gonna work till it learns to pick directions. Or go backwards
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        hitAsteroid = asteroid;
                    }
                }
            }

            //Return true if an intersection was found, false otherwise
            return hitAsteroid != null;
        }

        private bool RayIntersectsAsteroid(Vector2 origin, Vector2 direction, float length, Asteroid asteroid, out float distance)
        {
            distance = 0f;

            //Create the asteroid's bounding box
            var asteroidTransform = Matrix.CreateRotationZ(asteroid.rotation) * Matrix.CreateTranslation(new Vector3(asteroid.position, 0));
            var asteroidBounds = CreateBoundingBox(asteroid.vertices, asteroidTransform);

            //Create a ray from the origin point and direction
            Ray ray = new Ray(new Vector3(origin, 0), new Vector3(direction, 0));

            //Check if the ray intersects with the asteroid's bounding box
            //The ? just means it can be null
            float? intersect = ray.Intersects(asteroidBounds);
            //If there is an intersection and it is within the specified length, set the distance and return true
            if (intersect.HasValue && intersect.Value <= length)
            {
                distance = intersect.Value;
                return true;
            }

            return false;
        }

        //Helper to create a bounding box
        private BoundingBox CreateBoundingBox(VertexPositionColor[] vertices, Matrix transform)
        {
            //Transform the vertices of the object using the provided transformation matrix
            var transformedVertices = vertices.Select(v => Vector3.Transform(v.Position, transform)).ToArray();
            //Create and return a bounding box that contains all the transformed vertices
            return BoundingBox.CreateFromPoints(transformedVertices);
        }


    }

}
