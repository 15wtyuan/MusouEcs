using System;
using UnityEngine;

namespace MusouEcs
{
    public sealed class ObbShape : Shape
    {
        public Vector2 Center;
        public Vector2 Size;
        public Vector2 Right;

        private readonly Vector2 _originalCenter;
        private readonly Vector2 _oriSize;
        private readonly Vector2 _oriRight;
        private Vector2 _originalPoint;

        public ObbShape(Vector2 aabbMin, Vector2 aabbMax)
        {
            _originalCenter = (aabbMin + aabbMax) / 2;
            _oriSize = (aabbMax - aabbMin);
            _oriRight = Vector2.right;
            Reset();
        }

        internal override ShapeType GetShapeType()
        {
            return ShapeType.Obb;
        }

        internal override void Reset()
        {
            Center = _originalCenter;
            Size = _oriSize;
            Right = _oriRight;
            _originalPoint = Vector2.zero;
        }

        internal override void Translate(Vector2 delta)
        {
            Center += delta;
            _originalPoint += delta;
        }

        internal override void Rotate(float degrees)
        {
            Right = Right.RotatedByDegrees(degrees);
            Center = Center.RotatedByDegrees(degrees, _originalPoint);
        }

        internal override void FlipSide()
        {
            var curOriginalPoint = _originalPoint;
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);
            //反转都是以原点进行翻转，因为设计上都是以原点作为设计锚点
            Center = new Vector2(-Center.x, Center.y);
            Right.x = -Right.x;

            Translate(curOriginalPoint);
        }

        internal override void FlipUpDown()
        {
            var curOriginalPoint = _originalPoint;
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);
            //反转都是以原点进行翻转，因为设计上都是以原点作为设计锚点
            Center = new Vector2(Center.x, -Center.y);
            Right.y = -Right.y;

            Translate(curOriginalPoint);
        }

        internal override void Scale(Vector2 scale)
        {
            Size *= scale;

            var centerDelta = Center - _originalPoint;
            centerDelta *= scale;
            Center = _originalPoint + centerDelta;
        }

        internal override bool Intersects(Shape otherShape)
        {
            switch (otherShape.GetShapeType())
            {
                case ShapeType.AABB:
                {
                    var shape = otherShape as AABBShape;
                    return ShapeTools.OBBIntersectsAABB(Center, Size, Right, shape!.AABBMin, shape.AABBMax);
                }
                case ShapeType.Circle:
                {
                    var shape = otherShape as CircleShape;
                    return ShapeTools.OBBIntersectsCircle(Center, Size, Right, shape!.Center, shape.CircleRadius);
                }
                case ShapeType.Polygon:
                {
                    var shape = otherShape as PolygonShape;
                    return ShapeTools.OBBIntersectsPolygon(Center, Size, Right, shape!.PolygonVertices);
                }
                case ShapeType.Point:
                {
                    var shape = otherShape as PointShape;
                    return ShapeTools.OBBContainsPoint(Center, Size, Right, shape!.Center);
                }
                case ShapeType.Obb:
                {
                    var shape = otherShape as ObbShape;
                    return ShapeTools.OBBIntersectsOBB(Center, Size, Right, shape!.Center, shape.Size, shape.Right);
                }
                case ShapeType.Sector:
                {
                    var shape = otherShape as SectorShape;
                    return ShapeTools.SectorIntersectsOBB(shape!.Center, shape.Radius, shape.Dir, shape.Theta, Center,
                        Size, Right);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal override bool Intersects(Vector2 point)
        {
            return ShapeTools.OBBContainsPoint(Center, Size, Right, point);
        }

        internal override void RenderShape(Vector2[] cachedVertices)
        {
            Gizmos.color = RenderColor;
            var numVertices = 4;
            ShapeTools.GetVerticesOBB(Center, Size, Right, cachedVertices);

            for (var i = 0; i < numVertices; i++)
            {
                var nextIndex = (i + 1) % numVertices;
                Gizmos.DrawLine(cachedVertices[i], cachedVertices[nextIndex]);
            }
        }
    }
}