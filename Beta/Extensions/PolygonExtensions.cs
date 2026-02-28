using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Shapes;

namespace Beta.Extensions;

public static class PolygonExtensions
{
    public static bool ContainsWithTolerance(this Polygon polygon, Vector2 point)
    {
        var epsilon = .5f;

        var isInside = false;

        if (polygon.Vertices.Length < 3)
        {
            return false;
        }

        var oldPoint = polygon.Vertices[polygon.Vertices.Length - 1];
        var oldSqDist = (point - oldPoint).LengthSquared();

        for (var i = 0; i < polygon.Vertices.Length; ++i)
        {
            var newPoint = polygon.Vertices[i];
            var newSqDist = (point - newPoint).LengthSquared();

            if (oldSqDist + newSqDist + 2 * Math.Sqrt(oldSqDist * newSqDist)
                - (oldPoint - newPoint).LengthSquared() < epsilon)
            {
                return true;
            }

            Vector2 left, right;
            if (newPoint.X > oldPoint.X)
            {
                left = oldPoint;
                right = newPoint;
            }
            else
            {
                left = newPoint;
                right = oldPoint;
            }

            if (left.X < point.X
                && point.X <= right.X
                && (point.Y - left.Y) * (right.X - left.X) < (right.Y - left.Y) * (point.X - left.X))
            {
                isInside = !isInside;
            }

            oldPoint = newPoint;
            oldSqDist = newSqDist;
        }

        return isInside;
    }

    public static float DistanceToSegment(
        this Polygon polygon,
        Vector2 p,
        Vector2 v,
        Vector2 w
    )
    {
        return (float)Math.Sqrt(polygon.DistanceToSegmentSquared(p, v, w));
    }

    public static float DistanceToSegmentSquared(
        this Polygon polygon,
        Vector2 p,
        Vector2 v,
        Vector2 w
    )
    {
        var lensq = (w-v).LengthSquared();
        
        if (lensq == 0)
        {
            return (v-p).LengthSquared();
        }

        var t = ((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / lensq;

        if (t < 0)
        {
            return (v-p).LengthSquared();
        }
        if (t > 1)
        {
            return (w-p).LengthSquared();
        }

        var m = v + t*(w - v);
        return (m-p).LengthSquared();
    }

    public static Tuple<Vector2, Vector2> GetClosestEdge(this Polygon polygon, Vector2 to)
    {
        var vi1 = -1;
        var vi2 = -1;

        float mindist = float.MaxValue;

        for (int i = 0; i < polygon.Vertices.Length; ++i)
        {
            var dist = polygon.DistanceToSegment(
                to,
                polygon.Vertices[i],
                polygon.Vertices[(i+1) % polygon.Vertices.Length]
            );
            if (dist < mindist)
            {
                mindist = dist;
                vi1 = i;
                vi2 = (i + 1) % polygon.Vertices.Length;
            }
        }

        var p1 = polygon.Vertices[vi1];
        var p2 = polygon.Vertices[vi2];

        return Tuple.Create(p1, p2);
    }

    public static Vector2 GetClosestEdgePoint(this Polygon polygon, Vector2 to)
    {
        var (p1, p2) = GetClosestEdge(polygon, to);

        float u =
            (to - p1).Dot(p2 - p1) 
            /
            (p2-p1).LengthSquared();

        if (u < 0)
        {
            return p1;
        }
        if (u > 1)
        {
            return p2;
        }

        return p1 + u*(p2-p1);
    }
}

