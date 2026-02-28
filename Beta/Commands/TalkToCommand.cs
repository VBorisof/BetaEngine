using System;
using Beta.Actors;
using Beta.DI;
using Beta.GameStates;
using Beta.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Beta.Commands;

public class TalkToCommand : ActorCommand
{
    private readonly ILogger _logger;
    private readonly Actor _other;
    private readonly int _nodeIndex;
    private readonly GameStateManager _gameStateManager;
    private EventHandler _onComplete = (_, __) => { };

    public TalkToCommand(Actor actor, Actor other, int nodeIndex) : base(actor)
    {
        SkipStyle = CommandSkipStyle.Disabled;
        _other = other;
        _nodeIndex = nodeIndex;
        _gameStateManager = DependencyContainer.Instance.Get<GameStateManager>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
    }


    public override void Startup()
    {
        if (_other.Dialogue is null)
        {
            _logger.Error($"Missing dialogue for {_other.Name}");
            IsDone = true;
            return;
        }
        _gameStateManager.RequestStateDialogue(_other.Dialogue, _nodeIndex);
    }

    public void Then(EventHandler then)
    {
        _onComplete += then;
    }

    public override bool Update(GameTime gameTime)
    {
        IsDone = true;
        return true;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
    }

    public override void OnComplete()
    {
        base.OnComplete();
        _logger.Debug("");
        _onComplete(this, EventArgs.Empty);
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        _logger.Debug("");
    }
}
