using System;
using Beta.DI;
using Beta.Logging;
using Beta.Scenes;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class SetSceneCommand : Command
{
    private readonly ILogger _logger;
    private readonly SceneManager _sceneManager;
    private readonly Action _onComplete;

    public string SceneName { get; }

    public SetSceneCommand(string sceneName, Action onComplete)
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        SceneName = sceneName;
        _onComplete = onComplete;
    }

    public override void Startup()
    {
        _logger.Debug($"Set scene to {SceneName}");
        _sceneManager.SetScene(SceneName);
        IsDone = true;
    }

    public override bool Update(GameTime gameTime)
    {
        return IsDone;
    }

    public override void OnComplete()
    {
        base.OnComplete();
        _logger.Debug("");
        _onComplete();
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        _logger.Debug("");
    }
}

