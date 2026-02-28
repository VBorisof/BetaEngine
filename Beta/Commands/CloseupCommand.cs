using Beta.DI;
using Beta.GameStates;
using Beta.Logging;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class CloseupCommand : Command
{
    public string Name { get; }

    private readonly GameStateManager _gameStateManager;
    private readonly ILogger _logger;

    public CloseupCommand(string name)
    {
        Name = name;
        _gameStateManager = DependencyContainer.Instance.Get<GameStateManager>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public override void Startup()
    {
        _gameStateManager.RequestStateOverlay(Name);
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
