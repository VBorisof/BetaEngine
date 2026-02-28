namespace Beta.Scenes.Pathfinding;

public class GraphEdge
{
    public int From { get; set; }
    public int To { get; set; }
    public float Cost { get; set; }

    public GraphEdge(int from, int to, float cost)
    {
        From = from;
        To = to;
        Cost = cost;
    }

    public GraphEdge(GraphEdge other)
    {
        From = other.From;
        To = other.To;
        Cost = other.Cost;
    }
}