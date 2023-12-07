using UnityEngine;

namespace MusouEcs
{
    public class ShapeHelper : MonoBehaviour
    {
        [Header("图形类型")] public ShapeType type;
        [Header("圆半径，图形类型为圆时生效")] public float circleRadius;
        [Header("中心位置，图形类型为圆时生效")] public Vector2 center;

        [Header("扇形中轴方向，图形类型为扇形时生效")] public Vector2 dir;
        [Header("扇形半角，必须是0~90，图形类型为扇形时生效")] public float degreesTheta;

        [Header("多边形点集合，图形类型为多边形时生效")] public Vector2[] polygonVertices;
        [Header("矩形左下角点，图形类型为矩形时生效")] public Vector2 aabbMin;
        [Header("矩形右上角点，图形类型为矩形时生效")] public Vector2 aabbMax;

        [Space(20)] [Header("图形旋转角度")] public float rotateDegrees;
        [Header("图形左右反转")] public bool flipSide;
        [Header("图形上下反转")] public bool flipUpDown;
        [Header("图形缩放")] public int scale = 100;
        [Header("图形位移")] public Vector2 translateDelta;

        private static readonly Vector2[] CachedVertices = new Vector2[32];

        public Shape GetShape()
        {
            Shape shape;
            switch (type)
            {
                case ShapeType.Point:
                    shape = new PointShape(center);
                    break;
                case ShapeType.AABB:
                    shape = new AABBShape(aabbMin, aabbMax);
                    break;
                case ShapeType.Obb:
                    shape = new ObbShape(aabbMin, aabbMax);
                    break;
                case ShapeType.Circle:
                    shape = new CircleShape(center, circleRadius);
                    break;
                case ShapeType.Polygon:
                    shape = new PolygonShape(polygonVertices);
                    break;
                case ShapeType.Sector:
                    shape = new SectorShape(center, dir.normalized, circleRadius, degreesTheta);
                    break;
                default:
                    return null;
            }

            shape.Translate(translateDelta);
            shape.Rotate(rotateDegrees);
            if (flipSide)
            {
                shape.FlipSide();
            }

            if (flipUpDown)
            {
                shape.FlipUpDown();
            }

            shape.Scale(new Vector2(scale / 100f, scale / 100f));
            return shape;
        }

        public void OnDrawGizmos()
        {
            var shape = GetShape();
            shape?.RenderShape(ShapeHelper.CachedVertices);
        }
    }
}