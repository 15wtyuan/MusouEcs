using System;
using UnityEngine;

namespace MusouEcs
{
    public sealed class PointShape : Shape
    {
        public Vector2 Center;

        private readonly Vector2 _oriCenter;
        private Vector2 _originalPoint;

        internal PointShape(Vector2 center)
        {
            _oriCenter = center;
            Reset();
        }

        internal override ShapeType GetShapeType()
        {
            return ShapeType.Point;
        }

        internal override void Reset()
        {
            Center = _oriCenter;
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
        }

        internal override bool Intersects(Shape otherShape)
        {
            switch (otherShape.GetShapeType())
            {
                case ShapeType.AABB:
                {
                    var shape = otherShape as AABBShape;
                    return ShapeTools.AABBContainsPoint(shape!.AABBMin, shape.AABBMax, Center);
                }
                case ShapeType.Circle:
                {
                    var shape = otherShape as CircleShape;
                    return ShapeTools.CircleContainsPoint(shape!.Center, shape.CircleRadius, Center);
                }
                case ShapeType.Polygon:
                {
                    var shape = otherShape as PolygonShape;
                    return ShapeTools.PolygonContainsPoint(shape!.PolygonVertices, Center);
                }
                case ShapeType.Point:
                    return false;
                case ShapeType.Obb:
                {
                    var shape = otherShape as ObbShape;
                    return ShapeTools.OBBContainsPoint(shape!.Center, shape.Size, shape.Right, Center);
                }
                case ShapeType.Sector:
                {
                    var shape = otherShape as SectorShape;
                    return ShapeTools.SectorContainsPoint(shape!.Center, shape.Radius, shape.Dir, shape.Theta, Center);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal override bool Intersects(Vector2 point)
        {
            return false;
        }


        internal override void RenderShape(Vector2[] cachedVertices)
        {
            Gizmos.color = RenderColor;
            Gizmos.DrawWireSphere(Center, 0.1f);
        }
    }
}