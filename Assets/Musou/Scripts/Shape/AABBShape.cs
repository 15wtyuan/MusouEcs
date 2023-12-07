using System;
using UnityEngine;

namespace MusouEcs
{
    public sealed class AABBShape : Shape
    {
        public Vector2 AABBMin;
        public Vector2 AABBMax;

        private Vector2 _originalPoint;
        private readonly Vector2 _oriAABBMin;
        private readonly Vector2 _oriAABBMax;

        internal AABBShape(Vector2 aabbMin, Vector2 aabbMax)
        {
            _oriAABBMax = aabbMax;
            _oriAABBMin = aabbMin;
            Reset();
        }

        internal override ShapeType GetShapeType()
        {
            return ShapeType.AABB;
        }

        internal override void Reset()
        {
            AABBMin = _oriAABBMin;
            AABBMax = _oriAABBMax;
            _originalPoint = Vector2.zero;
        }

        internal override void Translate(Vector2 delta)
        {
            AABBMin += delta;
            AABBMax += delta;
            _originalPoint += delta;
        }

        internal override void Rotate(float degrees)
        {
            //不能旋转
        }

        internal override void FlipSide()
        {
            var curOriginalPoint = _originalPoint;
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);
            //反转都是以原点进行翻转，因为设计上都是以原点作为设计锚点
            var oriAABBMin = new Vector2(AABBMin.x, AABBMin.y);
            AABBMin = new Vector2(-AABBMax.x, AABBMin.y);
            AABBMax = new Vector2(-oriAABBMin.x, AABBMax.y);

            Translate(curOriginalPoint);
        }

        internal override void FlipUpDown()
        {
            var curOriginalPoint = _originalPoint;
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);
            //反转都是以原点进行翻转，因为设计上都是以原点作为设计锚点
            var oriAABBMin = new Vector2(AABBMin.x, AABBMin.y);
            AABBMin = new Vector2(AABBMin.x, -AABBMax.y);
            AABBMax = new Vector2(AABBMax.x, -oriAABBMin.y);

            Translate(curOriginalPoint);
        }

        internal override void Scale(Vector2 scale)
        {
            var curOriginalPoint = _originalPoint;
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);

            AABBMax *= scale;
            AABBMin *= scale;

            Translate(curOriginalPoint);
        }

        internal override bool Intersects(Shape otherShape)
        {
            switch (otherShape.GetShapeType())
            {
                case ShapeType.AABB:
                {
                    var shape = otherShape as AABBShape;
                    return ShapeTools.AABBIntersectsAABB(AABBMin, AABBMax, shape!.AABBMin, shape.AABBMax);
                }
                case ShapeType.Circle:
                {
                    var shape = otherShape as CircleShape;
                    return ShapeTools.CircleIntersectsAABB(shape!.Center, shape.CircleRadius, AABBMin, AABBMax);
                }
                case ShapeType.Polygon:
                {
                    var shape = otherShape as PolygonShape;
                    return ShapeTools.PolygonIntersectsAABB(shape!.PolygonVertices, AABBMin, AABBMax);
                }
                case ShapeType.Point:
                {
                    var shape = otherShape as PointShape;
                    return ShapeTools.AABBContainsPoint(AABBMin, AABBMax, shape!.Center);
                }
                case ShapeType.Obb:
                {
                    var shape = otherShape as ObbShape;
                    return ShapeTools.OBBIntersectsAABB(shape!.Center, shape.Size, shape.Right, AABBMin, AABBMax);
                }
                case ShapeType.Sector:
                {
                    var shape = otherShape as SectorShape;
                    return ShapeTools.SectorIntersectsAABB(shape!.Center, shape.Radius, shape.Dir, shape.Theta, AABBMin,
                        AABBMax);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal override bool Intersects(Vector2 point)
        {
            return ShapeTools.AABBContainsPoint(AABBMin, AABBMax, point);
        }

        internal override void RenderShape(Vector2[] cachedVertices)
        {
            Gizmos.color = RenderColor;
            const int numVertices = 4;
            ShapeTools.GetVerticesAABB(AABBMin, AABBMax, cachedVertices);

            for (var i = 0; i < numVertices; i++)
            {
                var nextIndex = (i + 1) % numVertices;
                Gizmos.DrawLine(cachedVertices[i], cachedVertices[nextIndex]);
            }
        }
    }
}