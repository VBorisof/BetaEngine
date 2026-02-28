using BDSM.Runtime;
using Beta.CommandManagement;
using Beta.DI;
using Beta.GameStates;
using Beta.Logging;
using Beta.Scenes;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

/* TODO: Probably remove this command. */
public class EndGameCommand : Command
{
    private readonly CommandManager _commandManager;
    private readonly SceneManager _sceneManager;
    private readonly Driver _bdsmDriver;
    private readonly GameStateManager _gameStateManager;
    private readonly ILogger _logger;

    public EndGameCommand()
    {
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _bdsmDriver = DependencyContainer.Instance.Get<Driver>();
        _gameStateManager = DependencyContainer.Instance.Get<GameStateManager>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public override void Startup()
    {
        _commandManager.Interrupt(interruptAsync: true);
        _bdsmDriver.Interpreter.RestartEnvironment();

        _sceneManager.NullScene();

        _gameStateManager.Fade.Remove();
        _gameStateManager.RequestStateMainMenu();

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
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        _logger.Debug("");
    }
}
