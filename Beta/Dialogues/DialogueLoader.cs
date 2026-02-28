using System.IO;
using System.Text.Json;
using Beta.DI;
using Beta.Logging;
using System.Text.Json.Serialization;
using Beta.ContentTools;
using Beta.Common;

namespace Beta.Dialogues;

[JsonSerializable(typeof(DialogueModel))]
[JsonSerializable(typeof(DialogueNode))]
[JsonSerializable(typeof(DialogueEdge))]
[JsonSerializable(typeof(DialoguePhrase))]
internal partial class DialogueSourceGenerationContext : JsonSerializerContext
{
}

public class DialogueLoader
{
    private readonly ILogger _logger;
    private readonly IContentPathProvider _contentPathProvider;

    public DialogueLoader()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _contentPathProvider = DependencyContainer.Instance.Get<IContentPathProvider>();
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        TypeInfoResolver = DialogueSourceGenerationContext.Default
    };

    public DialogueModel? LoadOrDefault(string actorName)
    {
        var path = _contentPathProvider.ProvideDialoguePath(actorName);
        var json = "";
        try
        {
            json = FileLoader.ReadAllFromFile(path);
        }
        catch (IOException)
        {
            _logger.Info($"Didn't find dialogue for {actorName}");
            return null;
        }

        _logger.Info($"Success loading dialogue JSON for {actorName}");
#pragma warning disable IL3050, IL2026
        var d = JsonSerializer.Deserialize<DialogueModel>(json, _options);
#pragma warning restore IL3050, IL2026

        /*
         * TODO: Fix this. We add the entity AFTER we load dialogue, so cannot check this way...
         * Or have to adjust the flow.
        foreach (var node in d.Nodes)
        {
            foreach (var phrase in node.Phrases)
            {
                if (!_entityManager.Contains(phrase.Who))
                {
                    _logger.Error($"Failed to load dialogue for {name}: Entity not found: `{phrase.Who}`");
                    // TODO: Should probably be an exception.
                    return null;
                }
            }
        }
        */

        return d;
    }
}