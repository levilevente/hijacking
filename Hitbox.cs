using Silk.NET.Maths;

public class Hitbox
{
    private (int x, int y, int z) topLeft;
    private (int x, int y, int z) bottomRight;

    public Hitbox((int x, int y, int z) topLeft, (int x, int y, int z) bottomRight)
    {
        this.topLeft = topLeft;
        this.bottomRight = bottomRight;
    }

    public bool IsColliding(Hitbox other)
    {
        return this.topLeft.x < other.bottomRight.x &&
               this.bottomRight.x > other.topLeft.x &&
               this.topLeft.y < other.bottomRight.y &&
               this.bottomRight.y > other.topLeft.y &&
               this.topLeft.z < other.bottomRight.z &&
               this.bottomRight.z > other.topLeft.z;
    }
    public Hitbox Translated(Vector3D<float> translation)
    {
        var translatedTopLeft = (topLeft.x + (int)translation.X, topLeft.y + (int)translation.Y, topLeft.z + (int)translation.Z);
        var translatedBottomRight = (bottomRight.x + (int)translation.X, bottomRight.y + (int)translation.Y, bottomRight.z + (int)translation.Z);
        return new Hitbox(translatedTopLeft, translatedBottomRight);
    }
}