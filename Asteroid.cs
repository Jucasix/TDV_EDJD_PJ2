using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

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

    public void Update()
    {
        position += velocity;
        rotation += 0.01f; //Make asteroids rotate over time
    }

    public void Draw(GraphicsDevice graphicsDevice, BasicEffect basicEffect)
    {
        graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
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

    public void Reset(Vector2 position, Vector2 velocity)
    {
        //Reset the asteroid's properties
        this.position = position;
        this.velocity = velocity;
        this.rotation = 0f;
    }

}




