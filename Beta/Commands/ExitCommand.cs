using Beta.Actors;
using Beta.BDSM;
using Beta.DI;
using Beta.GameStates;
using Beta.Logging;
using Beta.Scenes;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class ExitCommand : ActorCommand
{
    private readonly BDSMAdapter _bdsmAdapter;
    private readonly ILogger _logger;
    private readonly GameStateManager _gameStateManager;

    public SceneExit Exit { get; }

    public ExitCommand(Actor actor, SceneExit exit) : base(actor)
    {
        SkipStyle = CommandSkipStyle.Disabled;
        Exit = exit;
        _bdsmAdapter = DependencyContainer.Instance.Get<BDSMAdapter>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _gameStateManager = DependencyContainer.Instance.Get<GameStateManager>();
    }

    public override void Startup()
    {
        _logger.Debug($"Exit scene via {Exit.StartIndex}->{Exit.TargetIndex}({Exit.Destination})");
        _bdsmAdapter.ExitScene(Exit);
        _gameStateManager.ResetState();
        IsDone = true;
    }

    public override bool Update(GameTime gameTime)
    {
        return IsDone;
    }

    public override void OnComplete()
    {
        base.OnComplete();
        _logger.Debug();
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        _logger.Debug();
    }
}
