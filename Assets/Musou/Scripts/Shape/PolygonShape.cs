using System;
using UnityEngine;

namespace MusouEcs
{
    public class PolygonShape : Shape
    {
        public Vector2[] PolygonVertices;

        private readonly Vector2[] _oriPolygonVertices;
        private Vector2 _originalPoint;

        internal PolygonShape(Vector2[] polygonVertices)
        {
            PolygonVertices = (Vector2[])polygonVertices.Clone();
            _oriPolygonVertices = (Vector2[])polygonVertices.Clone();
            _originalPoint = Vector2.zero;
        }

        internal override ShapeType GetShapeType()
        {
            return ShapeType.Polygon;
        }

        internal override void Reset()
        {
            _originalPoint = Vector2.zero;
            for (var i = 0; i < PolygonVertices.Length; i++)
                PolygonVertices[i] = _oriPolygonVertices[i];
        }


        internal override void Translate(Vector2 delta)
        {
            _originalPoint += delta;
            for (var i = 0; i < PolygonVertices.Length; i++)
                PolygonVertices[i] += delta;
        }

        internal override void Rotate(float degrees)
        {
            for (var i = 0; i < PolygonVertices.Length; i++)
                PolygonVertices[i] = PolygonVertices[i].RotatedByDegrees(degrees, _originalPoint);
        }

        internal override void FlipSide()
        {
            var curOriginalPoint = _originalPoint;
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);
            //反转都是以原点进行翻转，因为设计上都是以原点作为设计锚点
            var scale = new Vector2(-1, 1);
            for (var i = 0; i < PolygonVertices.Length; i++)
                PolygonVertices[i] *= scale;

            Translate(curOriginalPoint);
        }

        internal override void FlipUpDown()
        {
            var curOriginalPoint = _originalPoint;
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);
            //反转都是以原点进行翻转，因为设计上都是以原点作为设计锚点
            var scale = new Vector2(1, -1);
            for (var i = 0; i < PolygonVertices.Length; i++)
                PolygonVertices[i] *= scale;

            Translate(curOriginalPoint);
        }

        internal override void Scale(Vector2 scale)
        {
            var curOriginalPoint = _originalPoint;
            //先重置偏移，再放大
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);
            for (var i = 0; i < PolygonVertices.Length; i++)
            {
                PolygonVertices[i] *= scale;
            }

            Translate(curOriginalPoint);
        }

        internal override bool Intersects(Shape otherShape)
        {
            switch (otherShape.GetShapeType())
            {
                case ShapeType.AABB:
                {
                    var shape = otherShape as AABBShape;
                    return ShapeTools.PolygonIntersectsAABB(PolygonVertices, shape!.AABBMin, shape.AABBMax);
                }
                case ShapeType.Circle:
                {
                    var shape = otherShape as CircleShape;
                    return ShapeTools.PolygonIntersectsCircle(PolygonVertices, shape!.Center, shape.CircleRadius);
                }
                case ShapeType.Polygon:
                {
                    var shape = otherShape as PolygonShape;
                    return ShapeTools.PolygonIntersectsPolygon(PolygonVertices, shape!.PolygonVertices);
                }
                case ShapeType.Point:
                {
                    var shape = otherShape as PointShape;
                    return ShapeTools.PolygonContainsPoint(PolygonVertices, shape!.Center);
                }
                case ShapeType.Obb:
                {
                    var shape = otherShape as ObbShape;
                    return ShapeTools.OBBIntersectsPolygon(shape!.Center, shape.Size, shape.Right, PolygonVertices);
                }
                case ShapeType.Sector:
                {
                    var shape = otherShape as SectorShape;
                    return ShapeTools.SectorIntersectsPolygon(shape!.Center, shape.Radius, shape.Dir, shape.Theta,
                        PolygonVertices);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal override bool Intersects(Vector2 point)
        {
            return ShapeTools.PolygonContainsPoint(PolygonVertices, point);
        }


        internal override void RenderShape(Vector2[] cachedVertices)
        {
            Gizmos.color = RenderColor;
            var numVertices = PolygonVertices.Length;
            Array.Copy(PolygonVertices, cachedVertices, PolygonVertices.Length);

            for (var i = 0; i < numVertices; i++)
            {
                var nextIndex = (i + 1) % numVertices;
                Gizmos.DrawLine(cachedVertices[i], cachedVertices[nextIndex]);
            }
        }
    }
}