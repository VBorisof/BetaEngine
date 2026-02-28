using Beta.DI;
using Beta.Logging;
using Microsoft.Xna.Framework;
using System;

namespace Beta.Commands;

public class CommandLambda : Command
{
    public Action Action { get; set; }

    private readonly ILogger _logger;

    public CommandLambda(Action action)
    {
        Action = action;
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public override void Startup()
    {
        Action();
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

