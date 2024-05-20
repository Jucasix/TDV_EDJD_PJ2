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
        public VertexPositionColor[] Vertices;

        public int Faction = 2;

        public Enemy(Vector2 position, float speed, float fireRate, float hitPoints, VertexPositionColor[] vertices)
        {
            Position = position;
            Speed = speed;
            FireRate = fireRate;
            HitPoints = hitPoints;
            IsActive = true;
            fireTimer = 0f;
            Vertices = vertices;
        }

        public void Update(Vector2 playerPosition, GameTime gameTime)
        {
            if (!IsActive)
                return;

            //Calculate direction towards player
            Vector2 direction = playerPosition - Position;
            direction.Normalize();

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
        }

        private void FireBullet(Vector2 direction)
        {
            //Add a new bullet fired towards the player
            Game1.bullets.Add(new Bullet(Position, direction, 2));
        }

        public bool CheckCollision(VertexPositionColor[] otherVertices, Matrix otherTransform)
        {
            Matrix enemyTransform = Matrix.CreateRotationZ(Rotation) * Matrix.CreateTranslation(new Vector3(Position, 0));
            return SATCollision.CheckCollision(Vertices, enemyTransform, otherVertices, otherTransform);
        }


    }

}
