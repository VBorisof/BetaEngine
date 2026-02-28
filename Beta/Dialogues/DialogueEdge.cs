using System.Text.Json.Serialization;

namespace Beta.Dialogues;

public class DialogueEdge
{
    [JsonPropertyName("from")]
    public int From { get; set; }
    [JsonPropertyName("to")]
    public int To { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    [JsonPropertyName("sound")]
    public string Sound { get; set; } = string.Empty;
    [JsonPropertyName("script")]
    public string Script { get; set; } = string.Empty;
    [JsonPropertyName("condition")]
    public string Condition { get; set; } = string.Empty;
}

