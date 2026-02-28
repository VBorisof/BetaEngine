using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Beta.Extensions;
using Beta.Scenes.Pathfinding;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Shapes;

namespace Beta.Scenes;

public class SceneMeta
{
    [JsonPropertyName("exits")]
    public List<ExitModel> Exits { get; set; } = [];
    [JsonPropertyName("regions")]
    public List<RegionModel> Regions { get; set; } = [];
    [JsonPropertyName("props")]
    public List<PropModel> Props { get; set; } = [];
    [JsonPropertyName("walkableAreas")]
    public List<WalkableAreaModel> WalkableAreas { get; set; } = [];
    [JsonPropertyName("actors")]
    public List<ScenePlacementModel> Actors { get; set; } = [];
    [JsonPropertyName("lights")]
    public List<SceneLightModel> Lights { get; set; } = [];

    public Graph Walkgraph { get; set; } = new();
    public Graph LastFullGraph { get; set; } = new();
    private List<Polygon> _polygons = [];
    private List<Vector2> _concaveVertices = [];

    public void AddEndpointsToWalkgraph(Vector2 from, Vector2 to, out Graph graph, out int fromNodeIdx, out int toNodeIdx)
    {
        graph = new Graph(Walkgraph);

        if (!_polygons[0].ContainsWithTolerance(from))
        {
            from = _polygons[0].GetClosestEdgePoint(from);
        }
        if (!_polygons[0].ContainsWithTolerance(to))
        {
            to = _polygons[0].GetClosestEdgePoint(to);
        }

        // If there are more polys, clamp destination to the edge.
        for (var i = 1; i < _polygons.Count; ++i)
        {
            if (_polygons[i].ContainsWithTolerance(to))
            {
                to = _polygons[i].GetClosestEdgePoint(to);
                break;
            }
            if (_polygons[i].ContainsWithTolerance(from))
            {
                from = _polygons[i].GetClosestEdgePoint(from);
                break;
            }
        }

        var fromNode = new GraphNode(from);
        // We start with copy of graph, so start at walkgraph.Nodes.Count
        fromNodeIdx = graph.Nodes.Count;
        graph.AddNode(fromNode);

        for (var i = 0; i < _concaveVertices.Count; ++i)
        {
            if (InLineOfSight(from, _concaveVertices[i]))
            {
                graph.AddEdge(
                    new GraphEdge(
                        fromNodeIdx,
                        i,
                        (_concaveVertices[i] - from).Length()
                    )
                );
            }
        }

        var toNode = new GraphNode(to);
        toNodeIdx = graph.Nodes.Count;
        graph.AddNode(toNode);

        for (var i = 0; i < _concaveVertices.Count; ++i)
        {
            if (InLineOfSight(to, _concaveVertices[i]))
            {
                graph.AddEdge(
                    new GraphEdge(
                        i,
                        toNodeIdx,
                        (to - _concaveVertices[i]).Length()
                    )
                );
            }
        }

        if (InLineOfSight(from, to))
        {
            graph.AddEdge(new GraphEdge(fromNodeIdx, toNodeIdx, (from - to).Length()));
        }
    }

    public List<Vector2> MakePath(Vector2 from, Vector2 to, IPathfindingAlgorithm? algo = null)
    {
        AddEndpointsToWalkgraph(from, to, out Graph walkgraph, out int fromNodeIdx, out int toNodeIdx);
        LastFullGraph = walkgraph;

        if (algo == null)
        {
            algo = new AstarAlgorithm();
        }
        var endNode = algo.GetPath(walkgraph, fromNodeIdx, toNodeIdx);

        var points = new List<Vector2>();
        if (endNode == null)
        {
            return points;
        }

        while (endNode.Parent != null)
        {
            points.Add(endNode.Position);
            endNode = endNode.Parent;
        }
        points.Add(endNode.Position);
        points.Reverse();

        return points;
    }

    public Graph CreateGraph(List<SceneWalkableArea> walkableAreas)
    {
        Walkgraph = new Graph();
        _polygons = walkableAreas.Select(wa => wa.Polygon).ToList();
        _concaveVertices = new List<Vector2>();

        // Add every concave vertex to the graph.
        var firstPoly = _polygons.First();
        foreach (var poly in _polygons)
        {
            if (poly.Vertices.Length > 2)
            {
                for (int i = 0; i < poly.Vertices.Length; ++i)
                {
                    // If it's the first polygon, we want the concave vertices.
                    // Otherwise, we treat the rest as inside areas and we want
                    // non-concave vertices.

                    var isConcave = IsVertexConcave(poly.Vertices, i);

                    if ((poly == firstPoly && isConcave) || (poly != firstPoly && !isConcave))
                    {
                        _concaveVertices.Add(poly.Vertices[i]);
                        Walkgraph.AddNode(new GraphNode(poly.Vertices[i].ToPoint().ToVector2()));
                    }
                }
            }
        }

        for (var i = 0; i < _concaveVertices.Count; ++i)
        {
            for (var j = 0; j < _concaveVertices.Count; j++)
            {
                if (i == j) continue;
                var vi = _concaveVertices[i];
                var vj = _concaveVertices[j];

                if (InLineOfSight(vi, vj))
                {
                    Walkgraph.AddEdge(new GraphEdge(i, j, (vj-vi).Length()));
                }
            }
        }

        LastFullGraph = Walkgraph;
        return Walkgraph;
    }

    private bool IsVertexConcave(Vector2[] vertices, int idx)
    {
        var current = vertices[idx];
        var next = vertices[(idx+1) % vertices.Length];
        var prev = vertices[(idx == 0 ? vertices.Length-1 : idx-1)];

        var left = new Vector2(current.X - prev.X, current.Y - prev.Y);
        var right = new Vector2(next.X - current.X, next.Y - current.Y);

        var cross = (left.X * right.Y) - (left.Y * right.X);

        return cross < 0;
    }

    private bool InLineOfSight(Vector2 from, Vector2 to)
    {
        // In LOS if almost the same vertex
        if ((to - from).Length() == 0) 
        {
            return true;
        }

        var firstPoly = _polygons.First();

        // Not in LOS if any end is outside the first polygon.
        if (!firstPoly.ContainsWithTolerance(from) || !firstPoly.ContainsWithTolerance(to))
        {
            return false;
        }

        foreach (var polygon in _polygons)
        {
            for (int i = 0; i < polygon.Vertices.Length; ++i)
            {
                var v1 = polygon.Vertices[i];
                var v2 = polygon.Vertices[(i+1)%polygon.Vertices.Length];
                if (LineSegmentsCross(from, to, v1, v2))
                {
                    // Avoid rounding issues.
                    var epsilon = 0.1f;
                    if (polygon.DistanceToSegment(from, v1, v2) > epsilon
                        && polygon.DistanceToSegment(to, v1, v2) > epsilon)
                    {
                        return false;
                    }
                }
            }
        }

        { // Middle point of segment.
            var v = from + to;
            var v2 = new Vector2(v.X/2, v.Y/2);
            bool isInside = firstPoly.ContainsWithTolerance(v2);

            for (int i = 1; i < _polygons.Count; ++i)
            {
                if (_polygons[i].ContainsWithTolerance(v2))
                {
                    isInside = false;
                }
            }

            return isInside;

            /*
            foreach (var polygon in _polygons)
            {
                if (polygon.ContainsWithTolerance(v2))
                {
                    return true;
                }
            }*/
        }

        //return false;
    }

    private bool LineSegmentsCross(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        float denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));

        if (denominator == 0)
        {
            return false;
        }

        float numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));

        float numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));

        if (numerator1 == 0 || numerator2 == 0)
        {
            return false;
        }

        float r = numerator1 / denominator;
        float s = numerator2 / denominator;

        return (r > 0 && r < 1) && (s > 0 && s < 1);
    }
}