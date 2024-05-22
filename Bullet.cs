using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using TDJ2_Astroidz;

public class Bullet
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public bool IsActive { get; set; }
    private float speed = 20f;

    public int Faction { get; private set; }

    public Bullet(Vector2 position, Vector2 direction, int faction)
    {
        Position = position;
        Velocity = Vector2.Normalize(direction) * speed;
        IsActive = true;
        Faction = faction;
    }

    public void Update(Vector3 playerPos, float inactiveDist)
    {
        if (!IsActive)
            return;

        Position += Velocity;

        //Calculate distance from player to bullet
        float distanceToPlayer = Vector2.Distance(new Vector2(playerPos.X, playerPos.Y), Position);

        //Deactivate bullet if it goes off-screen (for simplicity)
        if (distanceToPlayer > inactiveDist)
        {
            IsActive = false;
        }

    }

    public void HandleCollisions(IEnumerable<Asteroid> asteroids, IEnumerable<Enemy> enemies, VertexPositionColor[] playerVertices, Matrix playerTransform, ref float playerHitPoints)
    {
        if (!IsActive) return;

        //Check collision with player
        if (Faction != 1 && SATCollision.CheckCollision(
                new VertexPositionColor[] {
                    new VertexPositionColor(new Vector3(Position, 0), Color.White),
                    new VertexPositionColor(new Vector3(Position + new Vector2(1, 0), 0), Color.White)
                },
                Matrix.Identity,
                playerVertices,
                playerTransform))
        {
            IsActive = false;
            playerHitPoints -= 10f;
            if (playerHitPoints < 0) playerHitPoints = 0;
        }

        //Check collision with asteroids
        foreach (var asteroid in asteroids)
        {
            if (!asteroid.IsActive) continue;

            Matrix asteroidTransform = Matrix.CreateRotationZ(asteroid.rotation) * Matrix.CreateTranslation(new Vector3(asteroid.position, 0));
            if (SATCollision.CheckCollision(
                    new VertexPositionColor[] {
                        new VertexPositionColor(new Vector3(Position, 0), Color.White),
                        new VertexPositionColor(new Vector3(Position + new Vector2(1, 0), 0), Color.White)
                    },
                    Matrix.Identity,
                    asteroid.vertices,
                    asteroidTransform))
            {
                IsActive = false;
                asteroid.Hitpoints -= 20f;
                if (asteroid.Hitpoints <= 0) asteroid.IsActive = false;
                break;
            }
        }

        //Check collision with enemies
        foreach (var enemy in enemies)
        {
            if (!enemy.IsActive) continue;

            Matrix enemyTransform = Matrix.CreateRotationZ(enemy.Rotation) * Matrix.CreateTranslation(new Vector3(enemy.Position, 0));
            if (SATCollision.CheckCollision(
                    new VertexPositionColor[] {
                    new VertexPositionColor(new Vector3(Position, 0), Color.White),
                    new VertexPositionColor(new Vector3(Position + new Vector2(1, 0), 0), Color.White)
                    },
                    Matrix.Identity,
                    enemy.Vertices,
                    enemyTransform))
            {
                IsActive = false;
                enemy.HitPoints -= 20f;
                if (enemy.HitPoints <= 0) enemy.IsActive = false;
                return;
            }
        }
    }

    public void Draw(GraphicsDevice graphicsDevice, BasicEffect basicEffect, Matrix viewMatrix)
    {
        var vertices = new VertexPositionColor[3];
        vertices[0] = new VertexPositionColor(new Vector3(Position, -5), Color.Purple);
        vertices[1] = new VertexPositionColor(new Vector3(Position + Velocity, 0), Color.Purple);
        vertices[2] = new VertexPositionColor(new Vector3(Position + Velocity, 0), Color.Purple);

        basicEffect.World = Matrix.Identity;
        basicEffect.View = viewMatrix;
        basicEffect.CurrentTechnique.Passes[0].Apply();

        graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
    }
}
