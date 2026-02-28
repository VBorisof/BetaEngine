using Beta.Actors;
using Beta.DI;
using Beta.Logging;
using Beta.Services;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class AddPlayerItemCommand : ActorCommand
{
    private readonly Actor _what;
    private readonly ILogger _logger;
    private readonly HistoryService _historyService;

    public AddPlayerItemCommand(Actor who, Actor what) : base(who)
    {
        _what = what;
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _historyService = DependencyContainer.Instance.Get<HistoryService>();
    }

    public override void Startup()
    {
        Actor.Inventory.AddItem(_what);
        IsDone = true;
    }

    public override bool Update(GameTime gameTime)
    {
        return IsDone;
    }

    private void AppendToHistory()
    {
        _historyService.Append($"<{_what.Name}> added.");
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