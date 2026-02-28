using System.IO;
using Beta.DI;
using Beta.Logging;
using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using Beta.Extensions.Models;
using Beta.Cursors;
using Beta.ContentTools;
using Beta.Common;

namespace Beta.Tutorials;

[JsonSerializable(typeof(Tutorial))]
[JsonSerializable(typeof(TutorialStep))]
[JsonSerializable(typeof(TutorialStepAction))]
[JsonSerializable(typeof(TutorialStepActionType))]
[JsonSerializable(typeof(TutorialStepInstruction))]
[JsonSerializable(typeof(Vector2Model))]
internal partial class TutorialGenerationContext : JsonSerializerContext
{
}

public class TutorialProvider
{
    private readonly ILogger _logger;
    private readonly IContentPathProvider _contentPathProvider;

    public TutorialProvider()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _contentPathProvider = DependencyContainer.Instance.Get<IContentPathProvider>();
    }

    private static readonly JsonSerializerOptions _options = new()
    {
        TypeInfoResolver = TutorialGenerationContext.Default
    };

    public Tutorial GetTutorial()
    {
        var path = _contentPathProvider.ProvideTutorialPath();
        var json = "";
        try
        {
            json = FileLoader.ReadAllFromFile(path);
        }
        catch (IOException)
        {
            _logger.Error($"Couldn't find the tutorial file.");
            throw new FileNotFoundException("Tutorial file not found.");
        }
        _logger.Info($"Success loading the tutorial file");
#pragma warning disable IL3050, IL2026
        var tutorial = JsonSerializer.Deserialize<Tutorial>(json, _options);
#pragma warning restore IL3050, IL2026

        return tutorial ?? throw new ArgumentException("Tutorial file could not be loaded.");
    }
}