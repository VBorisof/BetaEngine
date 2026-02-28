using Beta.DI;
using Beta.Logging;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

// TODO: Remove this command.

public class RequestTutorialCommand : Command
{
    private readonly ILogger _logger;

    public RequestTutorialCommand()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }

    public override void Startup()
    {
        _logger.Warning($"{nameof(RequestTutorialCommand)} is deprecated!");
        _logger.Warning($"NOT executing.");
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
