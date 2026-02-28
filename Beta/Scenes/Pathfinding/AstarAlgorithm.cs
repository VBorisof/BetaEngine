using System.Collections.Generic;
using System.Linq;

namespace Beta.Scenes.Pathfinding;

public class AstarAlgorithm : IPathfindingAlgorithm
{
    public GraphNode GetPath(Graph walkgraph, int fromNodeIdx, int toNodeIdx)
    {
        var closedList = new HashSet<GraphNode>();
        var openList = new PriorityQueue<GraphNode, float>();

        var startNode = walkgraph.Nodes[fromNodeIdx];
        var endNode = walkgraph.Nodes[toNodeIdx];

        startNode.F = startNode.G;
        openList.Enqueue(startNode, startNode.F);

        while (openList.Count > 0)
        {
            var current = openList.Dequeue();
            if (current == endNode)
            {
                return current;
            }

            if (closedList.Contains(current))
            {
                continue;
            }

            // TODO: Add edges to node itself.
            foreach (var edge in walkgraph.Edges[walkgraph.Nodes.IndexOf(current)])
            {
                var toNode = walkgraph.Nodes[edge.To];

                var gCost = toNode.G + edge.Cost;
                var hCost = (endNode.Position - toNode.Position).Length();

                if (!openList.UnorderedItems.Any(n => n.Element == toNode)
                    && !closedList.Any(n => n == toNode))
                {
                    toNode.Parent = current;
                    toNode.G = gCost;
                    toNode.F = gCost + hCost;
                    openList.Enqueue(toNode, toNode.F);
                }
                else if (gCost < toNode.G)
                {
                    toNode.Parent = current;
                    toNode.G = gCost;
                    toNode.F = gCost + hCost;

                    if (closedList.Any(n => n == toNode))
                    {
                        closedList.Remove(toNode);
                        openList.Enqueue(toNode, toNode.F);
                    }
                }
            }
            closedList.Add(current);
        }

        return endNode;
    }
}