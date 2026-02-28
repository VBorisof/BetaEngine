using Beta.Actors;
using Beta.DI;
using Beta.Logging;
using Beta.Services;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class RemovePlayerItemCommand : ActorCommand
{
    private readonly ILogger _logger;
    private readonly Actor _what;
    private readonly HistoryService _historyService;

    public RemovePlayerItemCommand(Actor who, Actor what) : base(who)
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _what = what;
        _historyService = DependencyContainer.Instance.Get<HistoryService>();
    }

    public override void Startup()
    {
        Actor.Inventory.RemoveItem(_what);
        IsDone = true;
    }

    public override bool Update(GameTime gameTime)
    {
        return IsDone;
    }

    private void AppendToHistory()
    {
        _historyService.Append($"<{_what.Name}> removed.");
    }

    public override void OnComplete()
    {
        base.OnComplete();
        AppendToHistory();
        _logger.Debug("");
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        AppendToHistory();
        _logger.Debug("");
    }
}