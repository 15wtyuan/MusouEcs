using UnityEngine;

namespace MusouEcs
{
    public enum ShapeType
    {
        AABB,
        Circle,
        Polygon,
        Obb,
        Sector,
        Point,
    }

    public abstract class Shape
    {
        protected static Color RenderColor = Color.green;

        internal abstract ShapeType GetShapeType();

        internal abstract void Reset();

        internal abstract void Translate(Vector2 delta);

        internal abstract void Rotate(float degrees);

        internal abstract void FlipSide();

        internal abstract void FlipUpDown();

        internal abstract void Scale(Vector2 scale);

        internal abstract bool Intersects(Shape otherShape);

        internal abstract bool Intersects(Vector2 point);

        internal abstract void RenderShape(Vector2[] cachedVertices);
    }
}