using Beta.DI;
using Beta.GameStates;
using Beta.Logging;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class CinematicEndCommand : Command
{
    private readonly ILogger _logger;
    private readonly GameStateManager _gameStateManager;

    public CinematicEndCommand()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _gameStateManager = DependencyContainer.Instance.Get<GameStateManager>();
    }

    public override void Startup()
    {
        _logger.Debug();
        _gameStateManager.RequestStatePlaying();
        IsDone = true;
    }
    public override bool Update(GameTime gameTime)
    {
        return true;
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

