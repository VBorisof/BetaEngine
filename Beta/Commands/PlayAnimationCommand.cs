using System;
using System.Linq;
using Beta.Actors;
using Beta.DI;
using Beta.Logging;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class PlayAnimationCommand : ActorCommand
{
    private readonly ILogger _logger;

    public string Name { get; }

    public PlayAnimationCommand(Actor actor, string name) : base(actor)
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        Name = name;
    }

    public override void Startup()
    {
        Actor.SuggestState(ActorState.PlayAnimation);
        Actor.CurrentAnimation = Actor.CurrentCostume.Animations
            .Single(a => string.Equals(a.Name, Name, StringComparison.OrdinalIgnoreCase));
    }

    public override bool Update(GameTime gameTime)
    {
        if (Actor.CurrentAnimation.IsDone)
        {
            IsDone = true;
            return true;
        }
        return false;
    }

    public override void OnComplete()
    {
        base.OnComplete();
        _logger.Debug("");

        // TODO: This should be behind a flag
        Actor.SuggestState(ActorState.Idle);
    }
    public override void OnInterrupt()
    {
        base.OnInterrupt();
        _logger.Debug("");
        OnComplete();
    }
}