using Beta.DI;
using Microsoft.Xna.Framework;
using Beta.Logging;
using Beta.GameStates;

namespace Beta.Commands;

public class FadeOutCommand : Command
{
    public double Speed { get; }
    private readonly GameStateManager _gameStateManager;
    private readonly ILogger _logger;

    public FadeOutCommand(double speed)
    {
        _gameStateManager = DependencyContainer.Instance.Get<GameStateManager>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
        Speed = speed;
    }

    public override void Startup()
    {
        _gameStateManager.RequestFadeOut(Speed);
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

