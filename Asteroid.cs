using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using TDJ2_Astroidz;
using System.Collections.Generic;

public class Asteroid
{
    public VertexPositionColor[] vertices;
    public Vector2 position;
    public Vector2 velocity;
    public float rotation;
    public bool IsActive { get; set; }
    public float Mass { get; set; } = 5.0f;

    public float Hitpoints = 1000f;

    public Asteroid(VertexPositionColor[] vertices, Vector2 position, Vector2 velocity)
    {
        this.vertices = vertices;
        this.position = position;
        this.velocity = velocity;
        this.rotation = 0f;
        IsActive = true;
    }

    public void Update(Vector3 playerPosition, float inactiveThreshold)
    {
        position += velocity;
        rotation += 0.01f; //Make asteroids rotate over time

        //Deactivate asteroid if it is too far from the player
        float distanceToPlayer = Vector2.Distance(new Vector2(playerPosition.X, playerPosition.Y), position);
        if (distanceToPlayer > inactiveThreshold)
        {
            IsActive = false;
        }
    }

    public void Draw(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
    {
        graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
    }

    public bool CheckCollisionPlayer(VertexPositionColor[] playerVertices, Matrix playerTransform, ref Vector3 inertia, float playerSpeed, float playerMass, ref float playerHitPoints, Vector3 playerPos)
    {
        Matrix asteroidTransform = Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(new Vector3(position, 0));

        if (SATCollision.CheckCollision(playerVertices, playerTransform, vertices, asteroidTransform))
        {
            Vector2 collisionNormal = Vector2.Normalize(new Vector2(playerPos.X, playerPos.Y) - position);

            //Reflect the player's inertia
            Vector3 playerVelocity = inertia * playerSpeed;
            Vector3 relativeVelocity = playerVelocity - new Vector3(velocity, 0);

            //Calculate the impact speed along the collision normal
            float impactSpeed = Vector3.Dot(relativeVelocity, new Vector3(collisionNormal, 0));

            if (impactSpeed > 0)
                return false; //Objects are separating, no need to adjust velocities

            //Calculate new velocities based on conservation of momentum
            float totalMass = playerMass + Mass;
            float impulse = 2 * impactSpeed / totalMass;

            Vector3 impulseVector = impulse * Mass * new Vector3(collisionNormal, 0);
            inertia -= impulseVector / playerSpeed;
            inertia *= 0.9f; //Dampen the inertia slightly to simulate energy loss

            //Ensure a minimum inertia to prevent stopping completely
            if (inertia.Length() < 0.1f)
            {
                inertia = new Vector3(collisionNormal * -0.5f, 0);
            }

            HandleCollision(collisionNormal, playerSpeed, playerMass);

            //Calculate the impact force (use the magnitude of the impulse vector)
            float impactForce = impulseVector.Length();
            //Reduce player's health based on the impact force
            float healthReduction = impactForce / 2;
            playerHitPoints -= healthReduction;
            if (playerHitPoints < 0) playerHitPoints = 0; //Ensure health doesn't go below 0
            Console.WriteLine(playerHitPoints.ToString());

            return true;
        }

        return false;
    }
    public void HandleCollision(Vector2 collisionNormal, float otherVelocity, float otherMass)
    {
        //Calculate relative velocity
        Vector2 relativeVelocity = velocity - otherVelocity * collisionNormal;

        //Calculate the impact speed
        float impactSpeed = Vector2.Dot(relativeVelocity, collisionNormal);

        if (impactSpeed > 0)
            return; //Objects are separating, no need to adjust velocities

        //Calculate new velocities based on the conservation of momentum
        float totalMass = Mass + otherMass;
        float impulse = 2 * impactSpeed / totalMass;

        velocity += impulse * otherMass * collisionNormal;
        velocity *= 0.9f; //Dampen the velocity slightly to simulate energy loss

        //Ensure a minimum velocity to prevent stopping completely
        if (velocity.Length() < 0.1f)
        {
            velocity = collisionNormal * -0.5f;
        }
    }

    public void CheckCollisionEnemy(Enemy enemy)
{
    if (!IsActive || !enemy.IsActive) return;

    Matrix asteroidTransform = Matrix.CreateRotationZ(rotation) * Matrix.CreateTranslation(new Vector3(position, 0));
    Matrix enemyTransform = Matrix.CreateRotationZ(enemy.Rotation) * Matrix.CreateTranslation(new Vector3(enemy.Position, 0));

    if (SATCollision.CheckCollision(vertices, asteroidTransform, enemy.Vertices, enemyTransform))
    {
        Vector2 collisionNormal = Vector2.Normalize(position - enemy.Position);

        //Calculate relative velocity
        Vector2 relativeVelocity = velocity - enemy.Velocity;

        //Calculate the impact speed along the collision normal
        float impactSpeed = Vector2.Dot(relativeVelocity, collisionNormal);

        if (impactSpeed > 0) return; //Objects are separating, no need to adjust velocities

        //Calculate new velocities based on conservation of momentum
        float totalMass = Mass + enemy.Mass;
        float impulse = (2 * impactSpeed / totalMass) * 0.1f; //Scale down the impulse

        velocity -= impulse * enemy.Mass * collisionNormal;
        enemy.Velocity += impulse * Mass * collisionNormal;

        //Dampen the velocities slightly to simulate energy loss
        velocity *= 0.8f; //Increase damping
        enemy.Velocity *= 0.8f; //Increase damping

        //Ensure a minimum velocity to prevent stopping completely
        if (velocity.Length() < 0.1f)
        {
            velocity = collisionNormal * -0.5f;
        }
        if (enemy.Velocity.Length() < 0.1f)
        {
            enemy.Velocity = collisionNormal * 0.5f;
        }

        //Apply damage to both asteroid and enemy
        float damage = 10f; //Arbitrary damage value
        Hitpoints -= damage;
        enemy.HitPoints -= damage;

        //Deactivate if hitpoints are below zero
        if (Hitpoints <= 0) IsActive = false;
        if (enemy.HitPoints <= 0) enemy.IsActive = false;
    }
}



    public void CheckCollisionAsteroid(Asteroid otherAsteroid)
    {
        Matrix transformA = Matrix.CreateRotationZ(this.rotation) * Matrix.CreateTranslation(new Vector3(this.position, 0));
        Matrix transformB = Matrix.CreateRotationZ(otherAsteroid.rotation) * Matrix.CreateTranslation(new Vector3(otherAsteroid.position, 0));

        if (SATCollision.CheckCollision(this.vertices, transformA, otherAsteroid.vertices, transformB))
        {
            Vector2 collisionNormal = Vector2.Normalize(this.position - otherAsteroid.position);

            //Calculate relative velocity
            Vector2 relativeVelocity = this.velocity - otherAsteroid.velocity;

            //Calculate the impact speed along the collision normal
            float impactSpeed = Vector2.Dot(relativeVelocity, collisionNormal);

            if (impactSpeed > 0)
                return; //Objects are separating, no need to adjust velocities

            //Calculate new velocities based on conservation of momentum
            float totalMass = this.Mass + otherAsteroid.Mass;
            float impulse = 2 * impactSpeed / totalMass;

            this.velocity -= impulse * otherAsteroid.Mass * collisionNormal;
            otherAsteroid.velocity += impulse * this.Mass * collisionNormal;

            //Dampen the velocities slightly to simulate energy loss
            this.velocity *= 0.9f;
            otherAsteroid.velocity *= 0.9f;

            //Ensure a minimum velocity to prevent stopping completely
            if (this.velocity.Length() < 0.1f)
            {
                this.velocity = collisionNormal * -0.5f;
            }
            if (otherAsteroid.velocity.Length() < 0.1f)
            {
                otherAsteroid.velocity = collisionNormal * 0.5f;
            }
        }
    }

    public void Reset(Vector2 position, Vector2 velocity)
    {
        //Reset the asteroid's properties
        this.position = position;
        this.velocity = velocity;
        this.rotation = 0f;
    }

}




