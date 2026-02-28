using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Beta.Actors;

namespace Beta.Dialogues;

public record DialogueModel
{
    [JsonPropertyName("nodes")]
    public List<DialogueNode> Nodes { get; set; } = [];
    [JsonPropertyName("edges")]
    public List<DialogueEdge> Edges { get; set; } = [];
}

public record Dialogue
{
    public required Actor Actor { get; init; }
    public required List<DialogueNode> Nodes { get; init; }
    public required List<DialogueEdge> Edges { get; init; }

    public DialogueNode GetNodeById(int id)
    {
        return Nodes.Single(n => n.Id == id);
    }

    public List<DialogueEdge> GetOptions(DialogueNode node)
    {
        return Edges.Where(n => n.From == node.Id).ToList();
    }

    public bool IsNodeTerminal(DialogueNode node)
    {
        return !Edges.Any(e => e.From == node.Id);
    }
}