using Beta.ContentTools;
using Beta.DI;
using Microsoft.Xna.Framework.Content;
using System;
using System.IO;

namespace ExampleGame;

public class ContentPathProvider : IContentPathProvider
{
    private readonly ContentManager _content;

    public ContentPathProvider()
    {
        _content = DependencyContainer.Instance.Get<ContentManager>();
    }

    public string ProvideSaveGamePath()
    {
        return Path.Combine(AppContext.BaseDirectory, "saves");
    }

    public string ProvideSceneMetaPath(string sceneName)
    {
        return Path.Combine(
            _content.RootDirectory,
                "resources",
                    "scenes",
                        sceneName,
                            $"{sceneName}.json");
    }

    public string ProvideDepthMapPath(string sceneName)
    {
        return Path.Combine(
            "scenes",
                sceneName,
                    $"{sceneName}.depthmap");
    }

    public string ProvideDialoguePath(string actorName)
    {
        return Path.Combine(
            _content.RootDirectory,
                "resources",
                    "actors",
                        actorName,
                            $"{actorName}.dialogue.json");
    }

    public string ProvideEntityPath(string entityName)
    {
        return Path.Combine(
            _content.RootDirectory,
                "resources",
                    "actors",
                        entityName,
                            $"{entityName}.actor.json");
    }

    public string ProvideScaleMapPath(string sceneName)
    {
        return Path.Combine(
            "scenes",
                sceneName,
                    $"{sceneName}.scalemap");
    }

    public string ProvideSceneTexturePath(string sceneName)
    {
        return Path.Combine(
            "scenes",
                sceneName,
                    sceneName);
    }

    public string ProvideScriptsPath()
    {
        return Path.Combine(
            _content.RootDirectory,
                "scripts");
    }

    public string ProvideLayoutsPath()
    {
        return Path.Combine(
            _content.RootDirectory,
                "layouts");
    }

    public string ProvideTutorialPath()
    {
        return Path.Combine(
            _content.RootDirectory,
                "resources",
                    "tutorial.json");
    }
}
