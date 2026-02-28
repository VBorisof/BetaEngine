using System.Collections.Generic;
using System.Linq;

namespace Beta.Scenes.Pathfinding;

public class Graph
{
    // NB: Synced lists
    public List<GraphNode> Nodes { get; set; } = [];
    public List<List<GraphEdge>> Edges { get; set; } = [];

    public Graph()
    {
    }

    public Graph(Graph other)
    {
        Nodes = [];
        Edges = [];
        foreach (var node in other.Nodes)
        {
            Nodes.Add(new GraphNode(node));
        }
        foreach (var edges in other.Edges)
        {
            List<GraphEdge> clonedEdges = [];
            foreach (var edge in edges)
            {
                clonedEdges.Add(new GraphEdge(edge));
            }
            Edges.Add(clonedEdges);
        }
    }

    public GraphEdge? GetEdge(int from, int to)
    {
        var fromEdges = Edges[from];
        return fromEdges.SingleOrDefault(e => e.To == to);
    }

    public void AddNode(GraphNode node)
    {
        Nodes.Add(node);
        Edges.Add([]);
    }

    public void AddEdge(GraphEdge edge)
    {
        if (GetEdge(edge.From, edge.To) == null)
        {
            Edges[edge.From].Add(edge);
        }
        if (GetEdge(edge.To, edge.From) == null)
        {
            Edges[edge.To].Add(new GraphEdge(edge.To, edge.From, edge.Cost));
        }
    }
}