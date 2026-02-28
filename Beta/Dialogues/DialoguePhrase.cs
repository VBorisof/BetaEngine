using System.Text.Json.Serialization;

namespace Beta.Dialogues;

public class DialoguePhrase
{
    [JsonPropertyName("who")]
    public string Who { get; set; } = string.Empty;
    [JsonPropertyName("what")]
    public string What { get; set; } = string.Empty;
}


