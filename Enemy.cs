using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public int Faction = 2;
        public int EnemyType;

        public Enemy(Vector2 position, float speed, float fireRate, float hitPoints, VertexPositionColor[] vertices, int enemyType)
        {
            Position = position;
            Speed = speed;
            FireRate = fireRate;
            HitPoints = hitPoints;
            IsActive = true;
            fireTimer = 0f;
            Vertices = vertices;
            EnemyType = enemyType;
            Mass = 0.01f;
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
                // Avoidance maneuver
                Vector2 avoidanceDirection = Vector2.Transform(direction, Matrix.CreateRotationZ(MathHelper.PiOver2));
                direction = (avoidanceDirection * 7.5f + direction * 0.5f);
                direction.Normalize();
            }

            //Move towards player
            Velocity = direction * Speed;
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Update rotation to face the player
            Rotation = (float)Math.Atan2(direction.Y, direction.X);

            //Update firing logic
            fireTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (fireTimer >= FireRate)
            {
                FireBullet(direction);
                fireTimer = 0f;
            }

            // Check and handle collision with player
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
                float enemyMass = 1.0f; // Assuming enemy mass is also 1 for simplicity
                float totalMass = playerMass + enemyMass;
                float impulse = 2 * impactSpeed / totalMass;
                Vector3 impulseVector = impulse * enemyMass * new Vector3(collisionNormal, 0);
                inertia -= impulseVector / playerSpeed;
                inertia *= 0.9f;

                if (inertia.Length() < 0.1f)
                {
                    inertia = new Vector3(collisionNormal * -0.5f, 0);
                }

                // Reduce player's health based on the impact force
                float impactForce = impulseVector.Length();
                float healthReduction = impactForce / 10;
                playerHitPoints -= healthReduction;
                if (playerHitPoints < 0) playerHitPoints = 0;
                Console.WriteLine(playerHitPoints.ToString());

                // Handle enemy damage if necessary
                HitPoints -= healthReduction;
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

                if (RayIntersectsAsteroid(origin, direction, length, asteroid, out float distance))
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        hitAsteroid = asteroid;
                    }
                }
            }

            return hitAsteroid != null;
        }

        private bool RayIntersectsAsteroid(Vector2 origin, Vector2 direction, float length, Asteroid asteroid, out float distance)
        {
            distance = 0f;

            // Create the asteroid's bounding box
            var asteroidTransform = Matrix.CreateRotationZ(asteroid.rotation) * Matrix.CreateTranslation(new Vector3(asteroid.position, 0));
            var asteroidBounds = CreateBoundingBox(asteroid.vertices, asteroidTransform);

            // Create the ray
            Ray ray = new Ray(new Vector3(origin, 0), new Vector3(direction, 0));

            // Check for intersection
            float? intersect = ray.Intersects(asteroidBounds);
            if (intersect.HasValue && intersect.Value <= length)
            {
                distance = intersect.Value;
                return true;
            }

            return false;
        }

        private BoundingBox CreateBoundingBox(VertexPositionColor[] vertices, Matrix transform)
        {
            var transformedVertices = vertices.Select(v => Vector3.Transform(v.Position, transform)).ToArray();
            return BoundingBox.CreateFromPoints(transformedVertices);
        }


    }

}
