using System;
using UnityEngine;

namespace MusouEcs
{
    public sealed class SectorShape : Shape
    {
        public float Radius;
        public Vector2 Center;
        public Vector2 Dir;
        public float Theta;

        private readonly float _oriRadius;
        private readonly Vector2 _oriCenter;
        private readonly Vector2 _oriDir;
        private readonly float _oriTheta;

        private Vector2 _originalPoint;

        internal SectorShape(Vector2 center, Vector2 dir, float radius, float degrees)
        {
            _oriRadius = radius;
            _oriCenter = center;
            _oriDir = dir.normalized;
            _oriTheta = degrees * (Mathf.PI / 180f);
            Reset();
        }

        internal override ShapeType GetShapeType()
        {
            return ShapeType.Sector;
        }

        internal override void Reset()
        {
            Center = _oriCenter;
            Radius = _oriRadius;
            Dir = _oriDir;
            Theta = _oriTheta;

            _originalPoint = Vector2.zero;
        }

        internal void SetPos(Vector2 pos)
        {
            Center = pos;
            _originalPoint = pos;
        }

        internal override void Translate(Vector2 delta)
        {
            Center += delta;
            _originalPoint += delta;
        }

        internal override void Rotate(float degrees)
        {
            Dir = Dir.RotatedByDegrees(degrees);
            Center = Center.RotatedByDegrees(degrees, _originalPoint);
        }

        internal override void FlipSide()
        {
            var curOriginalPoint = _originalPoint;
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);
            //反转都是以原点进行翻转，因为设计上都是以原点作为设计锚点
            Center = new Vector2(-Center.x, Center.y);
            Dir.x = -Dir.x;

            Translate(curOriginalPoint);
        }

        internal override void FlipUpDown()
        {
            var curOriginalPoint = _originalPoint;
            var delta = Vector2.zero - curOriginalPoint;
            Translate(delta);
            //反转都是以原点进行翻转，因为设计上都是以原点作为设计锚点
            Center = new Vector2(Center.x, -Center.y);
            Dir.y = -Dir.y;

            Translate(curOriginalPoint);
        }

        internal override void Scale(Vector2 scale)
        {
            //先重置偏移，再放大
            Radius *= scale.x;
        }

        internal override bool Intersects(Shape otherShape)
        {
            switch (otherShape.GetShapeType())
            {
                case ShapeType.AABB:
                {
                    var shape = otherShape as AABBShape;
                    return ShapeTools.SectorIntersectsAABB(Center, Radius, Dir, Theta, shape!.AABBMin, shape.AABBMax);
                }
                case ShapeType.Circle:
                {
                    var shape = otherShape as CircleShape;
                    return ShapeTools.SectorIntersectsCircle(Center, Radius, Dir, Theta, shape!.Center,
                        shape.CircleRadius);
                }
                case ShapeType.Polygon:
                {
                    var shape = otherShape as PolygonShape;
                    return ShapeTools.SectorIntersectsPolygon(Center, Radius, Dir, Theta, shape!.PolygonVertices);
                }
                case ShapeType.Point:
                {
                    var shape = otherShape as PointShape;
                    return ShapeTools.SectorContainsPoint(Center, Radius, Dir, Theta, shape!.Center);
                }
                case ShapeType.Obb:
                {
                    var shape = otherShape as ObbShape;
                    return ShapeTools.SectorIntersectsOBB(Center, Radius, Dir, Theta, shape!.Center, shape.Size,
                        shape.Right);
                }
                case ShapeType.Sector:
                {
                    var shape = otherShape as SectorShape;
                    return ShapeTools.SectorIntersectsSector(Center, Radius, Dir, Theta, shape!.Center, shape.Radius,
                        shape.Dir, shape.Theta);
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal override bool Intersects(Vector2 point)
        {
            return ShapeTools.SectorContainsPoint(Center, Radius, Dir, Theta, point);
        }

        internal override void RenderShape(Vector2[] cachedVertices)
        {
            Gizmos.color = RenderColor;

            var numVertices = 32;

            int index = 0;
            cachedVertices[index] = Center;
            index++;

            var d = Theta / 15f;
            for (int i = 15; i >= 1; i--)
            {
                cachedVertices[index] = Center + (Dir * Radius).RotatedByRadians(d * i);
                index++;
            }

            cachedVertices[index] = Center + (Dir * Radius);
            index++;

            for (int i = 1; i <= 15; i++)
            {
                cachedVertices[index] = Center + (Dir * Radius).RotatedByRadians(-d * i);
                index++;
            }

            for (var i = 0; i < numVertices; i++)
            {
                var nextIndex = (i + 1) % numVertices;
                Gizmos.DrawLine(cachedVertices[i], cachedVertices[nextIndex]);
            }
        }
    }
}