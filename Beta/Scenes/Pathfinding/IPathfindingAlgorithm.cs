namespace Beta.Scenes.Pathfinding;

public interface IPathfindingAlgorithm
{
    GraphNode GetPath(Graph walkgraph, int fromNodeIdx, int toNodeIdx);
}