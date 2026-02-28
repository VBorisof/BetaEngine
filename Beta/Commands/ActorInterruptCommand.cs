using Beta.Actors;
using Beta.CommandManagement;
using Beta.DI;
using Beta.Logging;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class ActorInterruptCommand : ActorCommand
{
    private readonly CommandManager _commandManager;
    private readonly ILogger _logger;

    public ActorInterruptCommand(Actor who) : base(who)
    {
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public override void Startup()
    {
        _commandManager.Interrupt(Actor);
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