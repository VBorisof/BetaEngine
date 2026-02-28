using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Beta.Dialogues;

public class DialogueNode
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("isSkip")]
    public bool IsSkip { get; set; }
    [JsonPropertyName("phrases")]
    public List<DialoguePhrase> Phrases { get; set; } = [];
    [JsonPropertyName("sound")]
    public string Sound { get; set; } = string.Empty;
}

