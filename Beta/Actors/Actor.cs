using System;
using System.Linq;
using Beta.Common;
using Beta.DI;
using Beta.Dialogues;
using Beta.Entities;
using Beta.GameInventory;
using Beta.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Beta.Actors;

public class Actor : Entity
{
    public Inventory Inventory { get; } = new();
    private readonly ILogger _logger;

    public ActorState State { get; private set; } = ActorState.Idle;

    public Dialogue? Dialogue { get; set; }

    public EventHandler OnPickup { get; set; } = (sender, args) => { };

    public Color SpeechColor { get; set; }
    public SoundEffect? SpeechSound { get; }

    public Actor(string name) : base(name)
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();

        var dialogueModel = new DialogueLoader().LoadOrDefault(name);
        if (dialogueModel is not null)
        {
            Dialogue = new Dialogue
            {
                Actor = this,
                Edges = dialogueModel.Edges,
                Nodes = dialogueModel.Nodes
            };
        }

        var contentCache = DependencyContainer.Instance.Get<ContentCache>();

        try
        {
            SpeechSound = contentCache.GetOrDefault<SoundEffect>($"sounds/actors/{name}.talk");
        }
        catch (Exception) // See SoundService
        {
            _logger.Warning("Failed to load sound.");
        }
    }

    public void SuggestState(ActorState state)
    {
        if (State.IsManuallyManaged)
        {
            return;
        }

        ForceState(state);
    }
    public void ForceState(ActorState state)
    {
        State = state;
        SetAnimation();
    }

    public void SetCostume(string name)
    {
        var costume = Data.Costumes.SingleOrDefault(c => c.Name == name);
        if (costume is null)
        {
            _logger.Warning($"Costume {name} was not found.");
            return;
        }
        CurrentCostume = costume;

        SetAnimation();
    }

    private void SetAnimation()
    {
        var anim =
            CurrentCostume.Animations.SingleOrDefault(a =>
                string.Equals(a.Name, State.Name, StringComparison.OrdinalIgnoreCase));

        CurrentAnimation =
            anim is not null
                ? anim
                : CurrentCostume.Animations
                    .Single(a => string.Equals(
                        a.Name,
                        ActorState.Idle.Name,
                        StringComparison.OrdinalIgnoreCase));
    }
}