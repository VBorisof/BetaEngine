using Microsoft.Xna.Framework;
using Beta.Entities;
using Beta.DI;
using Beta.Logging;

namespace Beta.Commands;

public class SetPositionCommand : Command
{
    public SetPositionCommand(Entity what, Vector2 position)
    {
        What = what;
        Position = position;
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public Entity What { get; }
    public Vector2 Position { get; }

    private readonly ILogger _logger;

    public override void Startup()
    {
        What.Position = Position;
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

