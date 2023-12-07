using System;
using System.Collections.Generic;
using UnityEngine;

namespace MusouEcs
{
    public static class ShapeTools
    {
        private static readonly Vector2[] Vertices = new Vector2[32];
        private static readonly Vector2[] Vectors1 = new Vector2[4];
        private static readonly Vector2[] Vectors2 = new Vector2[4];

        #region 杂项

        /// <param name="c">新坐标系原点</param>
        /// <param name="right">新坐标系x轴</param>
        /// <param name="point">要转换的点</param>
        public static Vector2 ConvertCoordinateSpace(Vector2 c, Vector2 right, Vector2 point)
        {
            Vector2 d = point - c;
            return new Vector2(Vector2.Dot(d, right), Vector2.Dot(d, new Vector2(-right.y, right.x)));
        }

        /// <summary>
        /// vertexs所有点 在 ab线段上没有相交
        /// </summary>
        public static bool NotVectorsProjectionCoincideLineSegment(Vector2[] vertexs, int len, Vector2 a, Vector2 b)
        {
            for (int i = 0; i < len; i++)
                vertexs[i] -= a;
            var axis = b - a;

            VertexProject(vertexs, len, axis, out float range1Min, out float range1Max);
            return range1Min > axis.sqrMagnitude || range1Max < 0;
        }

        /// <summary>
        /// vertexs1所有点和vertexs2所有点 在 axis 轴上没有相交
        /// </summary>
        public static bool NotVectorsProjectionIntersectInAxis(Vector2[] vertexs1, int len1, Vector2[] vertexs2, int len2, Vector2 axis)
        {
            VertexProject(vertexs1, len1, axis, out float range1Min, out float range1Max);
            VertexProject(vertexs2, len2, axis, out float range2Min, out float range2Max);
            return range1Min > range2Max || range1Max < range2Min;
        }

        /// <summary>
        /// 顶点在轴上的投影的最小值和最大值
        /// </summary>
        public static void VertexProject(Vector2[] vertexs, int len, Vector2 axis, out float min, out float max)
        {
            min = float.MaxValue; max = float.MinValue;
            for (int i = 0; i < len; ++i)
            {
                Vector2 vertex = vertexs[i];
                float dot = Vector2.Dot(vertex, axis);
                min = Math.Min(min, dot);
                max = Math.Max(max, dot);
            }
        }

        /// <summary>
        /// 获取点在同一条线上的投影组合而成的最长线段
        /// </summary>
        public static void GetLineSegment(Vector2[] vertexs, int len, Vector2 a1, Vector2 b1, out Vector2 a, out Vector2 b)
        {
            for (int i = 0; i < len; i++)
                vertexs[i] = ClosestPointOnLine(a1, b1, vertexs[i]);

            a = vertexs[0]; b = vertexs[1];

            if (Math.Abs(a.x - b.x) < float.Epsilon)
            {
                for (int i = 0; i < len; i++)
                {
                    if (a.y > vertexs[i].y)
                        a = vertexs[i];
                    if (b.y < vertexs[i].y)
                        b = vertexs[i];
                }
            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    if (a.x > vertexs[i].x)
                        a = vertexs[i];
                    if (b.x < vertexs[i].x)
                        b = vertexs[i];
                }
            }
        }

        /// <summary>
        /// 计算线段与点的最短平方距离
        /// </summary>
        public static float SegmentPointSqrDistance(Vector2 a, Vector2 b, Vector2 point)
        {
            float t = Vector2.Dot(point - a, b) / b.sqrMagnitude;
            return (point - (a + Mathf.Clamp(t, 0, 1) * b)).sqrMagnitude;
        }

        /// <summary>
        /// 计算线段上与点最短距离的点
        /// </summary>
        public static Vector2 ClosestPointOnLineSegment(Vector2 a, Vector2 b, Vector2 point)
        {
            var ap = point - a;
            var ab = b - a;
            var dist = Vector2.Dot(ap, ab) / ab.sqrMagnitude;
            if (dist < 0)
                return a;
            else if (dist > 1)
                return b;
            else
                return a + ab * dist;
        }

        /// <summary>
        /// 计算线上与点最短距离的点
        /// </summary>
        public static Vector2 ClosestPointOnLine(Vector2 a, Vector2 b, Vector2 point)
        {
            var ap = point - a;
            var ab = b - a;
            var dist = Vector2.Dot(ap, ab) / ab.sqrMagnitude;
            return a + ab * dist;
        }

        public static bool PointIsOnLeftSideOfLine(Vector2 a, Vector2 b, Vector2 point) =>
            (b.x - a.x) * (point.y - a.y) - (b.y - a.y) * (point.x - a.x) > 0;

        public static bool LineIntersectsLine(Vector2 a1, Vector2 b1, Vector2 a2, Vector2 b2,
            out Vector2 intersection)
        {
            intersection = default;

            var x1 = a1.x;
            var x2 = b1.x;
            var x3 = a2.x;
            var x4 = b2.x;
            var y1 = a1.y;
            var y2 = b1.y;
            var y3 = a2.y;
            var y4 = b2.y;

            var d = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (Math.Abs(d) <= float.Epsilon)
                return false;

            var pre = x1 * y2 - y1 * x2;
            var post = x3 * y4 - y3 * x4;
            intersection.x = (pre * (x3 - x4) - (x1 - x2) * post) / d;
            intersection.y = (pre * (y3 - y4) - (y1 - y2) * post) / d;
            return true;
        }

        public static bool LineIntersectsLineSegment(Vector2 a1, Vector2 b1, Vector2 a2, Vector2 b2, out Vector2 intersection) =>
            LineIntersectsLine(a1, b1, a2, b2, out intersection) && IsInsideLineSegment(a2, b2, intersection);

        public static bool LineSegmentIntersectsLineSegment(Vector2 a1, Vector2 b1, Vector2 a2, Vector2 b2, out Vector2 intersection) =>
            LineIntersectsLineSegment(a1, b1, a2, b2, out intersection) && IsInsideLineSegment(a1, b1, intersection);

        private static bool IsInsideLineSegment(Vector2 a, Vector2 b, Vector2 point) =>
            (point.x >= a.x && point.x <= b.x || point.x >= b.x && point.x <= a.x) && (point.y >= a.y && point.y <= b.y || point.y >= b.y && point.y <= a.y);

        public static void GetPointGridPos(Vector2 center, float gridWidth, float gridHeight, ref Vector2Int gridPos)
        {
            gridPos.x = Mathf.FloorToInt(center.x / gridWidth);
            gridPos.y = Mathf.FloorToInt(center.y / gridHeight);
        }

        public static void CalPointOccupy(Vector2 center, float gridWidth, float gridHeight, HashSet<Vector2Int> occupy)
        {
            Vector2Int grid = Vector2Int.zero;
            GetPointGridPos(center, gridWidth, gridHeight, ref grid);
            occupy.Add(grid);
        }

        public static Vector2 RotatedByDegrees(this Vector2 v, float degrees) =>
            v.RotatedByDegrees(degrees, Vector2.zero);

        public static Vector2 RotatedByDegrees(this Vector2 v, float degrees, Vector2 center) =>
            v.RotatedByRadians(degrees * (Mathf.PI / 180f), center);

        public static Vector2 RotatedByRadians(this Vector2 v, float radians) => v.RotatedByRadians(radians, Vector2.zero);

        public static Vector2 RotatedByRadians(this Vector2 v, float radians, Vector2 center)
        {
            var cosTheta = Mathf.Cos(radians);
            var sinTheta = Mathf.Sin(radians);
            return new Vector2
            {
                x = cosTheta * (v.x - center.x) - sinTheta * (v.y - center.y) + center.x,
                y = sinTheta * (v.x - center.x) + cosTheta * (v.y - center.y) + center.y,
            };
        }

        public static float AngleBetween(this Vector2 v, Vector2 p) =>
            Mathf.Acos(Vector2.Dot(v, p) / (v.magnitude * p.magnitude));

        public static float AngleBetweenSigned(this Vector2 v, Vector2 p) =>
            v.AngleBetween(p) * (PointIsOnLeftSideOfLine(Vector2.zero, v, p) ? 1f : -1f);

        #endregion Vector2

        #region AABB

        public static bool AABBContainsPoint(Vector2 min, Vector2 max, Vector2 point) =>
            point.x >= min.x && point.x <= max.x && point.y >= min.y && point.y <= max.y;

        public static bool AABBIntersectsAABB(Vector2 minA, Vector2 maxA, Vector2 minB, Vector2 maxB) =>
            maxA.x - minA.x + maxB.x - minB.x > Mathf.Max(maxA.x, maxB.x) - Mathf.Min(minA.x, minB.x)
            && maxA.y - minA.y + maxB.y - minB.y > Mathf.Max(maxA.y, maxB.y) - Mathf.Min(minA.y, minB.y);

        public static void GetVerticesAABB(Vector2 min, Vector2 max, Vector2[] vertices)
        {
            vertices[0] = min;
            vertices[1] = new Vector2(min.x, max.y);
            vertices[2] = max;
            vertices[3] = new Vector2(max.x, min.y);
        }

        public static void CalAABBOccupy(Vector2 min, Vector2 max, float gridWidth, float gridHeight, HashSet<Vector2Int> occupy)
        {
            //找到min所占格子
            Vector2Int minGrid = new Vector2Int(Mathf.FloorToInt(min.x / gridWidth), Mathf.FloorToInt(min.y / gridHeight));
            Vector2Int maxGrid = new Vector2Int(Mathf.CeilToInt(max.x / gridWidth), Mathf.CeilToInt(max.y / gridHeight));
            CalAABBOccupyByAABBGrid(minGrid, maxGrid, occupy);
        }

        private static void CalAABBOccupyByAABBGrid(Vector2Int min, Vector2Int max, HashSet<Vector2Int> occupy)
        {
            for (int i = min.x; i <= max.x; i++)
            {
                for (int j = min.y; j < max.y; j++)
                {
                    occupy.Add(new Vector2Int(i, j));
                }
            }
        }
        #endregion AABB

        #region Circle

        public static bool CircleContainsPoint(Vector2 center, float radius, Vector2 point) =>
            (point - center).sqrMagnitude <= radius * radius;

        /// <summary>
        /// 分离轴方法
        /// </summary>
        public static bool CircleIntersectsAABB(Vector2 center, float radius, Vector2 aabbMin, Vector2 aabbMax)
        {
            Vector2 c = (aabbMin + aabbMax) / 2;
            Vector2 h = (aabbMax - aabbMin) / 2;
            return CircleIntersectsAABB(c, h, center, radius);
        }

        /// <param name="c">AABB的中心</param>
        /// <param name="h">AABB的半长度</param>
        /// <param name="p">圆盘的圆心</param>
        /// <param name="r">圆盘的半径</param>
        /// <returns></returns>
        static bool CircleIntersectsAABB(Vector2 c, Vector2 h, Vector2 p, float r)
        {
            Vector2 v = Vector2.Max(p - c, c - p); // = Abs(p - c);
            Vector2 u = Vector2.Max(v - h, Vector3.zero);
            return u.sqrMagnitude <= r * r;
        }

        public static bool CircleIntersectsCircle(Vector2 centerA, float radiusA, Vector2 centerB, float radiusB)
        {
            var radiusSum = radiusA + radiusB;
            return (centerB - centerA).sqrMagnitude <= radiusSum * radiusSum;
        }

        public static void GetAABBBoundingCircle(Vector2 center, float radius, out Vector2 aabbMin, out Vector2 aabbMax)
        {
            aabbMin = new Vector2(center.x - radius, center.y - radius);
            aabbMax = new Vector2(center.x + radius, center.y + radius);
        }

        public static void CalCircleOccupy(Vector2 center, float radius, float gridWidth, float gridHeight, HashSet<Vector2Int> occupy)
        {
            GetAABBBoundingCircle(center, radius, out Vector2 aabbMin, out Vector2 aabbMax);
            CalAABBOccupy(aabbMin, aabbMax, gridWidth, gridHeight, occupy);
        }

        #endregion Circle

        #region Polygon

        public static bool PolygonContainsPoint(Vector2[] vertices, Vector2 point)
        {
            var firstSide = PointIsOnLeftSideOfLine(vertices[0], vertices[1], point);
            for (var i = 1; i < vertices.Length; i++)
            {
                var nextIndex = (i + 1) % vertices.Length;
                if (firstSide != PointIsOnLeftSideOfLine(vertices[i], vertices[nextIndex], point))
                    return false;
            }

            return true;
        }

        public static bool PolygonIntersectsAABB(Vector2[] vertices, Vector2 aabbMin, Vector2 aabbMax)
        {
            return PolygonIntersectsAABB(vertices, vertices.Length, aabbMin, aabbMax);
        }

        public static bool PolygonIntersectsAABB(Vector2[] vertices, int length, Vector2 aabbMin, Vector2 aabbMax)
        {
            for (var i = 0; i < length; i++)
            {
                if (AABBContainsPoint(aabbMin, aabbMax, vertices[i]))
                    return true;
            }

            GetVerticesAABB(aabbMin, aabbMax, Vectors1);

            for (var i = 0; i < 4; i++)
            {
                if (PolygonContainsPoint(vertices, Vectors1[i]))
                    return true;
            }

            for (var i = 0; i < length; i++)
            {
                var a = vertices[i];
                var b = vertices[(i + 1) % length];
                for (var j = 0; j < 4; j++)
                {
                    var otherA = Vectors1[j];
                    var otherB = Vectors1[(j + 1) % 4];

                    if (LineSegmentIntersectsLineSegment(a, b, otherA, otherB, out _))
                        return true;
                }
            }

            return false;
        }

        public static bool PolygonIntersectsCircle(Vector2[] vertices, Vector2 circleCenter, float circleRadius)
        {
            if (PolygonContainsPoint(vertices, circleCenter))
                return true;

            var length = vertices.Length;
            for (var i = 0; i < length; i++)
            {
                var nextIndex = (i + 1) % length;
                var closestPoint = ClosestPointOnLineSegment(vertices[i], vertices[nextIndex], circleCenter);
                if (CircleContainsPoint(circleCenter, circleRadius, closestPoint))
                    return true;
            }

            return false;
        }

        public static bool PolygonIntersectsPolygon(Vector2[] verticesA, Vector2[] verticesB)
        {
            var lengthA = verticesA.Length;
            var lengthB = verticesB.Length;
            for (var i = 0; i < lengthA; i++)
            {
                if (PolygonContainsPoint(verticesB, verticesA[i]))
                    return true;
            }

            for (var i = 0; i < lengthB; i++)
            {
                if (PolygonContainsPoint(verticesA, verticesB[i]))
                    return true;
            }

            for (var i = 0; i < lengthA; i++)
            {
                var nextIndexA = (i + 1) % lengthA;
                for (var j = 0; j < lengthB; j++)
                {
                    var nextIndexB = (j + 1) % lengthB;
                    if (LineSegmentIntersectsLineSegment(verticesA[i], verticesA[nextIndexA], verticesB[j],
                            verticesB[nextIndexB], out _))
                        return true;
                }
            }

            return false;
        }

        public static void GetAABBBoundingPolygon(Vector2[] polygonVertices, out Vector2 aabbMin, out Vector2 aabbMax)
        {
            aabbMin = polygonVertices[0];
            aabbMax = polygonVertices[0];
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                aabbMin.x = Mathf.Min(polygonVertices[i].x, aabbMin.x);
                aabbMin.y = Mathf.Min(polygonVertices[i].y, aabbMin.y);

                aabbMax.x = Mathf.Max(polygonVertices[i].x, aabbMax.x);
                aabbMax.y = Mathf.Max(polygonVertices[i].y, aabbMax.y);
            }
        }

        public static void CalPolygonOccupy(Vector2[] polygonVertices, float gridWidth, float gridHeight, HashSet<Vector2Int> occupy)
        {
            GetAABBBoundingPolygon(polygonVertices, out Vector2 aabbMin, out Vector2 aabbMax);
            CalAABBOccupy(aabbMin, aabbMax, gridWidth, gridHeight, occupy);
        }

        #endregion

        #region OBB

        public static void CalOBBOccupy(Vector2 center, Vector2 size, Vector2 right, float gridWidth, float gridHeight, HashSet<Vector2Int> occupy)
        {
            GetVerticesOBB(center, size, right, Vectors1);
            GetAABBBoundingPolygon(Vectors1, out Vector2 aabbMin, out Vector2 aabbMax);
            CalAABBOccupy(aabbMin, aabbMax, gridWidth, gridHeight, occupy);
        }

        public static void GetVerticesOBB(Vector2 center, Vector2 size, Vector2 right, Vector2[] vertices)
        {
            Vector2 up = Vector2.Perpendicular(right).normalized;
            Vector2 hsize = size / 2;
            vertices[0] = center - right * hsize.x - up * hsize.y;
            vertices[1] = center + right * hsize.x - up * hsize.y;
            vertices[2] = center + right * hsize.x + up * hsize.y;
            vertices[3] = center - right * hsize.x + up * hsize.y;
        }

        public static void OBBConvertAABB(Vector2 size, out Vector2 min, out Vector2 max)
        {
            min = Vector2.zero - size / 2; max = Vector2.zero + size / 2;
        }

        public static bool OBBContainsPoint(Vector2 center, Vector2 size, Vector2 right, Vector2 point)
        {
            // 计算出Obb局部空间的 p
            Vector2 d = point - center;
            float px = Vector2.Dot(d, right);
            float py = Vector2.Dot(d, new Vector2(-right.y, right.x));

            OBBConvertAABB(size, out Vector2 min, out Vector2 max);
            return AABBContainsPoint(min, max, new Vector2(px, py));
        }

        public static bool OBBIntersectsAABB(Vector2 center, Vector2 size, Vector2 right, Vector2 min, Vector2 max)
        {
            GetVerticesOBB(center, size, right, Vectors1);
            GetVerticesAABB(min, max, Vectors2);
            // 如果有一个分离轴上不相交，则AABB 和 OBB 不相交
            return !(NotVectorsProjectionIntersectInAxis(Vectors1, 4, Vectors2, 4, Vector2.right)
                || NotVectorsProjectionIntersectInAxis(Vectors1, 4, Vectors2, 4, Vector2.up)
                || NotVectorsProjectionIntersectInAxis(Vectors1, 4, Vectors2, 4, right)
                || NotVectorsProjectionIntersectInAxis(Vectors1, 4, Vectors2, 4, Vector2.Perpendicular(right)));
        }

        public static bool OBBIntersectsCircle(Vector2 center, Vector2 size, Vector2 right, Vector2 centerC, float radius)
        {
            // 计算出Obb局部空间的 p
            Vector2 d = centerC - center;
            float px = Vector2.Dot(d, right);
            float py = Vector2.Dot(d, new Vector2(-right.y, right.x));

            OBBConvertAABB(size, out Vector2 min, out Vector2 max);
            return CircleIntersectsAABB(new Vector2(px, py), radius, min, max);
        }

        public static bool OBBIntersectsPolygon(Vector2 center, Vector2 size, Vector2 right, Vector2[] polygonVertices)
        {
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                Vertices[i] = ConvertCoordinateSpace(center, right, polygonVertices[i]);
            }
            OBBConvertAABB(size, out Vector2 min, out Vector2 max);
            return PolygonIntersectsAABB(Vertices, polygonVertices.Length, min, max);
        }

        public static bool OBBIntersectsOBB(Vector2 centerA, Vector2 sizeA, Vector2 rightA, Vector2 centerB, Vector2 sizeB, Vector2 rightB)
        {
            GetVerticesOBB(centerA, sizeA, rightA, Vectors1);
            GetVerticesOBB(centerB, sizeB, rightB, Vectors2);
            // 如果有一个分离轴上不相交，则OBB1 和 OBB2 不相交
            return !(NotVectorsProjectionIntersectInAxis(Vectors1, 4, Vectors2, 4, rightA)
                || NotVectorsProjectionIntersectInAxis(Vectors1, 4, Vectors2, 4, Vector2.Perpendicular(rightA))
                || NotVectorsProjectionIntersectInAxis(Vectors1, 4, Vectors2, 4, rightB)
                || NotVectorsProjectionIntersectInAxis(Vectors1, 4, Vectors2, 4, Vector2.Perpendicular(rightB)));
        }

        #endregion

        #region Sector
        public static void CalSectorOccupy(Vector2 center, float radius, Vector2 dir, float theta, float gridWidth, float gridHeight, HashSet<Vector2Int> occupy)
        {
            CalCircleOccupy(center, radius, gridWidth, gridHeight, occupy);
        }

        public static bool SectorContainsPoint(Vector2 center, float radius, Vector2 dir, float theta, Vector2 point) =>
            (point - center).sqrMagnitude <= radius * radius && Vector2.Angle(point - center, dir) * (Mathf.PI / 180f) <= theta;

        public static bool SectorIntersectsAABB(Vector2 center, float radius, Vector2 dir, float theta, Vector2 min, Vector2 max)
        {
            if (!CircleIntersectsAABB(center, radius, min, max))
            {
                return false;
            }

            GetVerticesAABB(min, max, Vectors2);
            return SectorIntersectsRectangle(center, radius, dir, theta, Vectors2);
        }

        public static bool SectorIntersectsRectangle(Vector2 center, float radius, Vector2 dir, float theta, Vector2[] rectangleVectors)
        {
            //以扇形圆心为原点，扇形的中心轴作为x轴，中心轴逆时针旋转90度作为y轴，创建一个坐标系，假设为a坐标系；将世界空间的矩形转换为坐标系a中的矩形，
            float allpy = 0;
            for (int i = 0; i < 4; i++)
            {
                Vector2 d = rectangleVectors[i] - center;
                float px = Vector2.Dot(d, dir);
                float py = Vector2.Dot(d, new Vector2(-dir.y, dir.x));
                rectangleVectors[i] = new Vector2(px, py);
                allpy += py;
            }
            //并将其转化为对称与x轴的第一、二象限的矩形；
            //算其点的平均值是否落在第一、二象限判断是否反转
            if (allpy < 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    rectangleVectors[i].y = -rectangleVectors[i].y;
                }
            }

            //依次判断扇形在矩形的长边和宽边上是否存在分离轴；
            if (!SectorProjectionIntersectsLineSegment(radius, theta, rectangleVectors[0], rectangleVectors[1]))
            {
                return false;
            }

            if (!SectorProjectionIntersectsLineSegment(radius, theta, rectangleVectors[1], rectangleVectors[2]))
            {
                return false;
            }

            //判断是否在扇形圆心与矩形距圆心最近点的连线上是否存在分离轴；
            float sqrCloserLen = float.MaxValue;
            Vector2 closerPoint = Vector2.zero;
            for (int i = 0; i < 4; i++)
            {
                Vector2 closerPointT = ClosestPointOnLineSegment(rectangleVectors[i], rectangleVectors[(i + 1) % 4], Vector2.zero);
                var sqrMagnitude = closerPointT.sqrMagnitude;
                if (sqrMagnitude < sqrCloserLen)
                {
                    closerPoint = closerPointT;
                    sqrCloserLen = sqrMagnitude;
                }
            }
            (Vectors1[0], Vectors1[1], Vectors1[2], Vectors1[3]) = (rectangleVectors[0], rectangleVectors[1], rectangleVectors[2], rectangleVectors[3]);
            GetLineSegment(Vectors1, 4, Vector2.zero, closerPoint, out Vector2 a1, out Vector2 b1);
            if (!SectorProjectionIntersectsLineSegment(radius, theta, a1, b1))
            {
                return false;
            }

            //判断扇形左边的的法线上是否存在分离轴(由于将矩形转化了第一、二象限的矩形，所以可以忽略扇形右边法线上分离轴的计算)；
            Vector2 leftPoint = (Vector2.right * radius).RotatedByRadians(theta);
            Vector2 leftNormal = Vector2.Perpendicular(leftPoint);

            (Vectors1[0], Vectors1[1], Vectors1[2], Vectors1[3]) = (rectangleVectors[0], rectangleVectors[1], rectangleVectors[2], rectangleVectors[3]);
            GetLineSegment(Vectors1, 4, Vector2.zero, leftNormal, out Vector2 a2, out Vector2 b2);
            if (!SectorProjectionIntersectsLineSegment(radius, theta, a2, b2))
            {
                return false;
            }

            //上述步骤中发现任意分离轴，则判定为未碰撞，反之，发生碰撞。
            return true;
        }

        static bool SectorProjectionIntersectsLineSegment(float radius, float theta, Vector2 a, Vector2 b)
        {
            Vector2 center = Vector2.zero;
            Vector2 dir = Vector2.right;
            //首先，计算线段所对应的正方向向量(线段终点 - 线段起点的方向)是否在扇形夹角区域内(即扇形圆心 + 此向量在扇形内)，
            //计算线段所对应的反方向向量(线段起点的方向 - 线段终点)是否在扇形夹角区域内；接下来分情况考虑。

            Vector2 pDir = a - b;//计算线段所对应的正方向向量(线段终点 - 线段起点的方向)
            Vector2 pPoint = center + pDir;

            Vector2 oDir = b - a;//计算线段所对应的反方向向量(线段起点的方向 - 线段终点)
            Vector2 oPoint = center + oDir;

            //如果线段所对应的正方向向量在扇形区域内，则计算以扇形圆心为起点，线段正方向为方向的射线与扇形外边的交点intersectPos1；
            bool containsP = Vector2.Angle(dir, pPoint) * (Mathf.PI / 180f) <= theta;
            Vector2 intersectPos1 = center + pDir.normalized * radius;

            //如果线段所对应的反方向向量在扇形区域内则计算以扇形圆心为起点，线段反方向为方向的射线与扇形外边的交点intersectPos2；
            bool containsO = Vector2.Angle(dir, oPoint) * (Mathf.PI / 180f) <= theta;
            Vector2 intersectPos2 = center + oDir.normalized * radius;

            //如果线段对应的正方向向量和反方向向量均在扇形区域内，则计算intersectPos1、intersectPos2点在线段的投影是否与线段相交即可；
            if (containsP && containsO)
            {
                (Vectors1[0], Vectors1[1]) = (intersectPos1, intersectPos2);
                return !NotVectorsProjectionCoincideLineSegment(Vectors1, 2, a, b);
            }
            else
            //如果线段对应的正方向向量和反方向向量均不在扇形区域内，则计算扇形圆心，扇形的左边顶点和扇形的右边顶点在线段的投影是否相交即可；
            if (!containsP && !containsO)
            {
                Vector2 leftPoint = center + (dir * radius).RotatedByRadians(theta);
                Vector2 rightPoint = center + (dir * radius).RotatedByRadians(-theta);

                (Vectors1[0], Vectors1[1], Vectors1[2]) = (center, leftPoint, rightPoint);
                return !NotVectorsProjectionCoincideLineSegment(Vectors1, 3, a, b);
            }
            else
            //如果仅线段对应的正方向向量在扇形区域内，则计算扇形圆心，intersectPos1，扇形的左边顶点和扇形的右边顶点在线段的投影是否相交即可；
            if (containsP)
            {
                Vector2 leftPoint = center + (dir * radius).RotatedByRadians(theta);
                Vector2 rightPoint = center + (dir * radius).RotatedByRadians(-theta);
                (Vectors1[0], Vectors1[1], Vectors1[2], Vectors1[3]) = (center, intersectPos1, leftPoint, rightPoint);
                return !NotVectorsProjectionCoincideLineSegment(Vectors1, 4, a, b);
            }
            else
            //如果仅线段对应的反方向向量在扇形区域内，则计算扇形圆心，intersectPos2，扇形的左边顶点和扇形的右边顶点在线段的投影是否相交即可；
            if (containsO)
            {
                Vector2 leftPoint = center + (dir * radius).RotatedByRadians(theta);
                Vector2 rightPoint = center + (dir * radius).RotatedByRadians(-theta);
                (Vectors1[0], Vectors1[1], Vectors1[2], Vectors1[3]) = (center, intersectPos2, leftPoint, rightPoint);
                return !NotVectorsProjectionCoincideLineSegment(Vectors1, 4, a, b);
            }
            throw new Exception();
        }

        public static bool SectorIntersectsCircle(Vector2 centerA, float radiusA, Vector2 dirA, float thetaA, Vector2 centerB, float radiusB)
        {
            Vector2 d = centerB - centerA;
            float rsum = radiusA + radiusB;
            if (d.sqrMagnitude > rsum * rsum)
                return false;

            float px = Vector2.Dot(d, dirA);
            float py = Mathf.Abs(Vector2.Dot(d, new Vector2(-dirA.y, dirA.x)));

            if (px > d.magnitude * Mathf.Cos(thetaA))
                return true;

            Vector2 q = radiusA * new Vector2(Mathf.Cos(thetaA), Mathf.Sin(thetaA));
            Vector2 p = new Vector2(px, py);
            return SegmentPointSqrDistance(Vector2.zero, q, p) <= radiusB * radiusB;
        }

        public static bool SectorIntersectsPolygon(Vector2 center, float radius, Vector2 dir, float theta, Vector2[] vertices)
        {
            //todo
            return false;
        }

        public static bool SectorIntersectsOBB(Vector2 center, float radius, Vector2 dir, float theta, Vector2 c, Vector2 size, Vector2 right)
        {
            if (!OBBIntersectsCircle(c, size, right, center, radius))
            {
                return false;
            }

            GetVerticesOBB(c, size, right, Vectors2);
            return SectorIntersectsRectangle(center, radius, dir, theta, Vectors2);
        }

        public static bool SectorIntersectsSector(Vector2 centerA, float radiusA, Vector2 dirA, float thetaA, Vector2 centerB, float radiusB, Vector2 dirB, float thetaB)
        {
            //todo
            return false;
        }

        #endregion
    }
}