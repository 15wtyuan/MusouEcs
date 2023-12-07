using System;
using UnityEngine;

namespace MusouEcs
{
    public sealed class CircleShape : Shape
    {
        public float CircleRadius;
        public Vector2 Center;

        private readonly float _oriCircleRadius;
        private readonly Vector2 _oriCircleCenter;
        private Vector2 _originalPoint;

        internal CircleShape(Vector2 center, float circleRadius)
        {
            _oriCircleCenter = center;
            _oriCircleRadius = circleRadius;
            Reset();
        }

        internal override ShapeType GetShapeType()
        {
            return ShapeType.Circle;
        }

        internal override void Reset()
        {
            Center = _oriCircleCenter;
            CircleRadius = _oriCircleRadius;
            _originalPoint = Vector2.zero;
        }

        internal override void Translate(Vector2 delta)
        {
            Center += delta;
            _originalPoint += delta;
        }

        internal override void Rotate(float degrees)
        {
            Center = Center.RotatedByDegrees(degrees, _originalPoint);
        }

        internal override void FlipSide()
        {
            var curOriginalPoint = _originalPoint;
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);
            //反转都是以原点进行翻转，因为设计上都是以原点作为设计锚点
            Center = new Vector2(-Center.x, Center.y);

            Translate(curOriginalPoint);
        }

        internal override void FlipUpDown()
        {
            var curOriginalPoint = _originalPoint;
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);
            //反转都是以原点进行翻转，因为设计上都是以原点作为设计锚点
            Center = new Vector2(Center.x, -Center.y);

            Translate(curOriginalPoint);
        }

        internal override void Scale(Vector2 scale)
        {
            CircleRadius *= scale.x;
        }

        internal override bool Intersects(Shape otherShape)
        {
            switch (otherShape.GetShapeType())
            {
                case ShapeType.AABB:
                {
                    var shape = otherShape as AABBShape;
                    return ShapeTools.CircleIntersectsAABB(Center, CircleRadius, shape!.AABBMin, shape.AABBMax);
                }
                case ShapeType.Circle:
                {
                    var shape = otherShape as CircleShape;
                    return ShapeTools.CircleIntersectsCircle(Center, CircleRadius, shape!.Center, shape.CircleRadius);
                }
                case ShapeType.Polygon:
                {
                    var shape = otherShape as PolygonShape;
                    return ShapeTools.PolygonIntersectsCircle(shape!.PolygonVertices, Center, CircleRadius);
                }
                case ShapeType.Point:
                {
                    var shape = otherShape as PointShape;
                    return ShapeTools.CircleContainsPoint(Center, CircleRadius, shape!.Center);
                }
                case ShapeType.Obb:
                {
                    var shape = otherShape as ObbShape;
                    return ShapeTools.OBBIntersectsCircle(shape!.Center, shape.Size, shape.Right, Center, CircleRadius);
                }
                case ShapeType.Sector:
                {
                    var shape = otherShape as SectorShape;
                    return ShapeTools.SectorIntersectsCircle(shape!.Center, shape.Radius, shape.Dir, shape.Theta,
                        Center,
                        CircleRadius);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal override bool Intersects(Vector2 point)
        {
            return ShapeTools.CircleContainsPoint(Center, CircleRadius, point);
        }


        internal override void RenderShape(Vector2[] cachedVertices)
        {
            Gizmos.color = RenderColor;
            Gizmos.DrawWireSphere(Center, CircleRadius);
        }
    }
}