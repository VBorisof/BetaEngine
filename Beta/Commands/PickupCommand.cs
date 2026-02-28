using System;
using Beta.Actors;
using Beta.DI;
using Beta.Logging;
using Beta.Services;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class PickupCommand : ActorCommand
{
    private readonly Actor _item;
    private readonly ILogger _logger;
    private readonly HistoryService _historyService;
    private EventHandler _onComplete = (_, __) => { };

    public PickupCommand(Actor actor, Actor item) : base(actor)
    {
        SkipStyle = CommandSkipStyle.Disabled;
        _item = item;
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _historyService = DependencyContainer.Instance.Get<HistoryService>();
    }

    public override void Startup()
    {
        Actor.Inventory.AddItem(_item);
        _item.Scene = null;
    }

    public override bool Update(GameTime gameTime)
    {
        // Make sure animation is done or something..?
        if (Actor.Inventory.Items.Contains(_item))
        {
            IsDone = true;
            _item.OnPickup(this, EventArgs.Empty);
            //_onComplete(this, null);
        }

        return IsDone;
    }

    public void Then(EventHandler then)
    {
        _onComplete += then;
    }

    private void AppendToHistory()
    {
        _historyService.Append($"Picked up <{_item.Name}>");
    }

    public override void OnComplete()
    {
        base.OnComplete();
        AppendToHistory();
        _logger.Debug("");
        _onComplete(this, EventArgs.Empty);
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        _logger.Debug("");
    }
}