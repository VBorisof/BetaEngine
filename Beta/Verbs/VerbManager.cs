using BDSM.ExecutionContexts;
using Beta.Actors;
using Beta.CommandManagement;
using Beta.Commands;
using Beta.Cursors;
using Beta.DI;
using Beta.Entities;
using Beta.GameStates;
using Beta.Input;
using Beta.InputMapping;
using Beta.Scenes;
using Beta.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Beta.Verbs;

public class VerbManager : IInputEventListener
{
    private readonly InputService _input;
    private readonly InputContextManager _inputContextManager;
    private readonly InputMapper _inputMapper;
    private readonly SceneManager _sceneManager;
    private readonly EntityManager _entityManager;
    private readonly CommandManager _commandManager;
    private readonly Cursor _cursor;
    private readonly TutorialService _tutorialService;

    public Verb SelectedVerb { get; private set; }

    public bool WasVerbExplicitlySelected { get; private set; }

    public VerbManager()
    {
        _input = DependencyContainer.Instance.Get<InputService>();
        _inputContextManager = DependencyContainer.Instance.Get<InputContextManager>();
        _inputMapper = DependencyContainer.Instance.Get<InputMapper>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
        _cursor = DependencyContainer.Instance.Get<Cursor>();
        _tutorialService = DependencyContainer.Instance.Get<TutorialService>();
        SelectedVerb = Verb.Walk;
    }

    public void RequestApplyCurrentVerb(Entity entity, Actor? item = null)
    {
        var args = EventArgs.Empty;
        if (item != null)
        {
            args = new UseVerbEventArgs(item);
        }

        entity.VerbHandlers[SelectedVerb](this, args);
        WasVerbExplicitlySelected = false;
        SelectWalk();
    }

    public void RequestApplyCurrentVerb(SceneProp prop, Actor? item = null)
    {
        var args = EventArgs.Empty;
        if (item != null)
        {
            args = new UseVerbEventArgs(item);
        }

        prop.VerbHandlers[SelectedVerb](this, args);
        WasVerbExplicitlySelected = false;
        SelectWalk();
    }

    public void RequestExit(SceneExit exit)
    {
        if (_entityManager.Player is null || _sceneManager.CurrentScene is null)
        {
            throw new InvalidOperationException("Scene not loaded correctly.");
        }

        List<Command> commands = [];
        var moveCommand = new MoveCommand(_entityManager.Player, new Vector2(exit.ExitPoint.X, exit.ExitPoint.Y));
        commands.Add(moveCommand);
        commands.Add(new ExitCommand(_entityManager.Player, exit));

        _commandManager.DispatchCommands(ExecutionContext.Actor(_entityManager.Player.DeclName), [.. commands]);
    }

    public void RequestMove(Vector2 scenePos)
    {
        if (_entityManager.Player is null || _sceneManager.CurrentScene is null)
        {
            throw new InvalidOperationException("Scene not loaded correctly.");
        }
        if (_commandManager.IsBusy(_entityManager.Player))
        {
            _commandManager.Skip(_entityManager.Player);
            if (_commandManager.IsBusy(_entityManager.Player))
            {
                return;
            }
        }

        var moveCommand = new MoveCommand(_entityManager.Player, scenePos);

        _commandManager.DispatchCommands(
            ExecutionContext.Actor(_entityManager.Player.DeclName),
            moveCommand
        );
    }

    public void ResetVerb()
    {
        SelectWalk();
        WasVerbExplicitlySelected = false;
    }

    public void SelectWalk()
    {
        SelectedVerb = Verb.Walk;
        _cursor.SetCursor(SelectedVerb);
    }

    public void SelectLook()
    {
        SelectedVerb = Verb.Look;
        _cursor.SetCursor(SelectedVerb);
    }

    public void SelectPickup()
    {
        SelectedVerb = Verb.Pickup;
        _cursor.SetCursor(SelectedVerb);
    }

    public void SelectInteract()
    {
        SelectedVerb = Verb.Interact;
        _cursor.SetCursor(SelectedVerb);
    }

    public void SelectTalk()
    {
        SelectedVerb = Verb.Talk;
        _cursor.SetCursor(SelectedVerb);
    }

    public void SelectUse()
    {
        SelectedVerb = Verb.Use;
        _cursor.SetCursor(SelectedVerb);
    }

    public void SelectExit()
    {
        SelectedVerb = Verb.Walk;
        _cursor.SetExit();
    }

    // See if we didn't select any particular verb
    // yet and try to guess what we want from context
    public void SuggestVerb(Verb verb)
    {
        if (!WasVerbExplicitlySelected)
        {
            switch (verb)
            {
                case Verb.Walk:
                    SelectWalk();
                    break;
                case Verb.Look:
                    SelectLook();
                    break;
                case Verb.Pickup:
                    SelectPickup();
                    break;
                case Verb.Talk:
                    SelectTalk();
                    break;
                case Verb.Use:
                    SelectUse();
                    break;
                case Verb.Interact:
                    SelectInteract();
                    break;
                default:
                    break;
            }
        }
    }
    public void SuggestExit()
    {
        if (!WasVerbExplicitlySelected)
        {
            SelectExit();
        }
    }

    public void OnSelectWalk()
    {
        WasVerbExplicitlySelected = true;
        SelectWalk();
    }
    public void OnSelectLook()
    {
        WasVerbExplicitlySelected = true;
        SelectLook();
    }
    public void OnSelectPickup()
    {
        WasVerbExplicitlySelected = true;
        SelectPickup();
    }
    public void OnSelectInteract()
    {
        WasVerbExplicitlySelected = true;
        SelectInteract();
    }
    public void OnSelectTalk()
    {
        WasVerbExplicitlySelected = true;
        SelectTalk();
    }

    public HashSet<InputContext> GetInputContexts()
    {
        return [
            _inputContextManager.GetOrCreateByName(nameof(GameStatePlaying)),
        ];
    }

    public InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        if (_tutorialService.IsInTutorial)
        {
            return new();
        }

        if (_inputMapper.IsMatch(args, GameInputType.DoWalk))
        {
            OnSelectWalk();
        }
        if (_inputMapper.IsMatch(args, GameInputType.DoLook))
        {
            OnSelectLook();
        }
        if (_inputMapper.IsMatch(args, GameInputType.DoPickup))
        {
            OnSelectPickup();
        }
        if (_inputMapper.IsMatch(args, GameInputType.DoInteract))
        {
            OnSelectInteract();
        }
        if (_inputMapper.IsMatch(args, GameInputType.DoTalk))
        {
            OnSelectTalk();
        }

        return new();
    }
}