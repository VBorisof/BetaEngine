using Beta.DI;
using Microsoft.Xna.Framework;
using Beta.Logging;
using Beta.GameStates;

namespace Beta.Commands;

public class CinematicStartCommand : Command
{
    private readonly ILogger _logger;
    private readonly GameStateManager _gameStateManager;

    public CinematicStartCommand()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _gameStateManager = DependencyContainer.Instance.Get<GameStateManager>();
    }

    public override void Startup()
    {
        _logger.Debug();
        _gameStateManager.RequestStateCinematic();
        IsDone = true;
    }
    public override bool Update(GameTime gameTime)
    {
        return true;
    }
}


