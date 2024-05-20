using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

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
