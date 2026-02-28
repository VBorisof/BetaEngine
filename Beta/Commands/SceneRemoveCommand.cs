using Beta.Actors;
using Microsoft.Xna.Framework;
using Beta.Entities;
using Beta.DI;
using Beta.Logging;

namespace Beta.Commands;

public class SceneRemoveCommand : Command
{
    private readonly Entity _what;
    private readonly ILogger _logger;

    public SceneRemoveCommand(Entity what)
    {
        _what = what;
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public override void Startup()
    {
        _what.Scene = null;
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
