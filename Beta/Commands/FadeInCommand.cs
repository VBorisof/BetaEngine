using Beta.DI;
using Beta.GameStates;
using Beta.Logging;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class FadeInCommand : Command
{
    public double Speed { get; }
    private readonly GameStateManager _gameStateManager;
    private readonly ILogger _logger;

    public FadeInCommand(double speed)
    {
        _gameStateManager = DependencyContainer.Instance.Get<GameStateManager>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
        Speed = speed;
    }

    public override void Startup()
    {
        _gameStateManager.RequestFadeIn(Speed);
        _gameStateManager.Fade.OnComplete += (_, __) => IsDone = true;
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

