using System;
using System.Collections.Generic;
using System.Linq;
using Beta.Actors;
using Beta.BDSM;
using Beta.DI;
using Beta.Logging;
using Microsoft.Xna.Framework;

namespace Beta.Commands;

public class MoveCommand : ActorCommand
{
    private readonly float _speedModifier = 500f;
    public Vector2 Destination { get; private set; }
    public Actor? DestinationActor { get; private set; }
    private int _currentPathIndex;
    private List<Vector2> _path = [];

    private readonly float _stopDistance;
    private readonly ILogger _logger;
    private readonly BDSMAdapter _bdsmAdapter;
    private EventHandler _onComplete = (_, __) => { };

    public MoveCommand(Actor actor, Vector2 destination, float stopDistance = 0) : base(actor)
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _bdsmAdapter = DependencyContainer.Instance.Get<BDSMAdapter>();
        _currentPathIndex = 0;
        Destination = destination;
        _stopDistance = stopDistance;
    }

    public MoveCommand(Actor actor, Actor destinationActor, float stopDistance = 0) : base(actor)
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _bdsmAdapter = DependencyContainer.Instance.Get<BDSMAdapter>();
        _currentPathIndex = 0;
        _stopDistance = stopDistance;
        DestinationActor = destinationActor;
    }

    public override void Startup()
    {
        if (Actor.Scene is null)
        {
            throw new InvalidOperationException("No scene defined.");
        }

        if (DestinationActor is not null)
        {
            Destination = DestinationActor.Position;
        }

        if (_stopDistance != 0)
        {
            var vec = Actor.Position - Destination;
            Destination += Vector2.Normalize(vec) * _stopDistance;
        }

        // TODO: Actually need to recalculate the whole path...
        _path = Actor.Scene.MakePath(Actor.Position, Destination);

        if (_path.Count == 0)
        {
            IsDone = true;
        }
        else
        {
            Destination = _path.Last();
            SetActorState();
        }
    }

    public void Then(EventHandler then)
    {
        _onComplete += then;
    }

    public override bool Update(GameTime gameTime)
    {
        if (IsDone)
        {
            return true;
        }

        var currentGoal = _path[_currentPathIndex];
        var dist = currentGoal - Actor.Position;

        var tolerance = gameTime.ElapsedGameTime.TotalSeconds * _speedModifier * Actor.Data.Speed;
        if (dist.Length() <= tolerance)// + _stopDistance)
        {
            ++_currentPathIndex;
            if (_currentPathIndex >= _path.Count)
            {
                IsDone = true;
                return true;
            }
            else
            {
                SetActorState();
                currentGoal = _path[_currentPathIndex];
                dist = currentGoal - Actor.Position;
            }
        }

        if (Actor.Position.X.Equals(float.NaN) || Actor.Position.Y.Equals(float.NaN))
        {
            _logger.Warning($"ActorPos was NAN: {Actor.Position}");
            Actor.Position = Destination;
            IsDone = true;
            //_onComplete(this, null);
            return true;
        }

        var dir = Vector2.Normalize(dist);

        var velocity = dir * (_speedModifier * (float)gameTime.ElapsedGameTime.TotalSeconds);
        Actor.MoveWithVelocity(velocity);

        CheckSceneRegions();

        return false;
    }

    // TODO: Do we need to handle the case where our path meets the region somewhere and 
    // some sort of callback needs to be called appropriately?
    private void CheckSceneRegions()
    {
        if (Actor.Scene is null)
        {
            throw new InvalidOperationException("No scene defined.");
        }

        var newRegionOrDefault = Actor.Scene.Regions
            .FirstOrDefault(r => r.Polygon.Contains(Actor.Position));

        // If actor wasn't in this region before,
        if (Actor.Region != newRegionOrDefault)
        {
            // Did we enter a new region?
            if (newRegionOrDefault is not null)
            {
                _bdsmAdapter.OnRegionEntered(Actor.Scene, newRegionOrDefault);
            }

            // If actor was in some region, we need to signal that we've left it.
            if (Actor.Region is not null && Actor.Scene.Regions.Contains(Actor.Region))
            {
                _bdsmAdapter.OnRegionExited(Actor.Scene, Actor.Region);
            }
        }
        Actor.Region = newRegionOrDefault;
    }

    // TODO: _onComplete should be replaced with Completed from base class.
    // Yes, yes, I know.
    public override void OnComplete()
    {
        base.OnComplete();
        _logger.Debug("");

        Actor.SuggestState(ActorState.Idle);
        _onComplete(this, EventArgs.Empty);

        CheckSceneRegions();
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
        _logger.Debug("");

        if (SkipStyle == CommandSkipStyle.SkipOne)
        {
            Actor.Position = Destination;
        }

        Actor.SuggestState(ActorState.Idle);
        _onComplete(this, EventArgs.Empty);

        CheckSceneRegions();
    }

    private void SetActorState()
    {
        var dest = _path[_currentPathIndex];

        var isMoveUp = Actor.Position.Y - dest.Y > 1;
        var isMoveDown = Actor.Position.Y - dest.Y < -1;
        var isMoveRight = dest.X - Actor.Position.X > 1;
        var isMoveLeft = dest.X - Actor.Position.X < -1;

        if (isMoveUp)
        {
            if (isMoveRight)
            {
                Actor.SuggestState(ActorState.WalkNorthEast);
            }
            else if (isMoveLeft)
            {
                Actor.SuggestState(ActorState.WalkNorthWest);
            }
            else
            {
                Actor.SuggestState(ActorState.WalkNorth);
            }
        }
        else if (isMoveDown)
        {
            if (isMoveRight)
            {
                Actor.SuggestState(ActorState.WalkSouthEast);
            }
            else if (isMoveLeft)
            {
                Actor.SuggestState(ActorState.WalkSouthWest);
            }
            else
            {
                Actor.SuggestState(ActorState.WalkSouth);
            }
        }
        else
        {
            if (isMoveRight)
            {
                Actor.SuggestState(ActorState.WalkEast);
            }
            else if (isMoveLeft)
            {
                Actor.SuggestState(ActorState.WalkWest);
            }
            // Another `else` here shouldn't make
            // sense -- it means we're not moving at all.
        }
    }
}