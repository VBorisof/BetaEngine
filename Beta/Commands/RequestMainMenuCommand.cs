using Beta.DI;
using Beta.GameStates;
using Beta.Logging;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

/* TODO: Probably remove this command or at least the `IsStarted` flag. */
public class RequestMainMenuCommand : Command
{
    public bool IsStarted { get; }

    private readonly GameStateManager _gameStateManager;
    private readonly ILogger _logger;

    public RequestMainMenuCommand(bool isStarted)
    {
        IsStarted = isStarted;
        _gameStateManager = DependencyContainer.Instance.Get<GameStateManager>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public override void Startup()
    {
        _gameStateManager.RequestStateMainMenu();
        IsDone = true;
    }

    public override bool Update(GameTime gameTime)
    {
        return IsDone;
    }
}
