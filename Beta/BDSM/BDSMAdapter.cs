using Beta.DI;
using Beta.Scenes;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Beta.Entities;
using System.Linq;
using BDSM.StandardLibrary;
using Beta.Logging;
using Beta.Commands;
using Beta.Common.Extensions;
using Beta.CommandManagement;
using Beta.Actors;
using BDSM.Language;
using Beta.Verbs;
using Beta.Phrases;
using BDSM.ExecutionContexts;
using Beta.Services;
using Beta.ContentTools;
using Beta.Services.Sounds;

namespace Beta.BDSM;

public class BDSMAdapter : BDSMEventHandler
{
    private readonly ILogger _logger;

    private readonly EntityManager _entityManager;
    private readonly SceneManager _sceneManager;
    private readonly CommandManager _commandManager;
    private readonly HistoryService _historyService;
    private readonly IContentPathProvider _contentPathProvider;
    private readonly MusicPlayerService _musicPlayerService;

    public EventHandler OnRequestCinematicStart { get; set; } = (_, __) => { };
    public EventHandler OnRequestCinematicEnd { get; set; } = (_, __) => { };
    public EventHandler OnRequestFadeIn { get; set; } = (_, __) => { };
    public EventHandler OnRequestFadeOut { get; set; } = (_, __) => { };

    public BDSMAdapter()
    {
        _historyService = DependencyContainer.Instance.Get<HistoryService>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
        _commandManager.RequestAsyncCallback += (_, asyncTag) =>
        {
            _bdsmDriver.Interpreter.RequestAsyncCallback(asyncTag);
        };

        _contentPathProvider = DependencyContainer.Instance.Get<IContentPathProvider>();

        _bdsmDriver.SetRootDir(_contentPathProvider.ProvideScriptsPath());

        _musicPlayerService = DependencyContainer.Instance.Get<MusicPlayerService>();

        Register();
    }

    public void Update(GameTime gameTime)
    {
        if (_sceneManager.CurrentScene == null)
        {
            return;
        }

        foreach (var entity in _entityManager.GetOnScene(_sceneManager.CurrentScene))
        {
            if (entity is Actor actor)
            {
                _logger.Trace($"Do we update {entity.DeclName}...");
                if (!_commandManager.IsBusy(actor))
                {
                    _logger.Trace($"{entity.DeclName} is not busy, call update.");
                    _bdsmDriver.Interpreter.TryCallUpdate(actor.DeclName);
                }
            }
        }
    }

    protected override void OnSetScene(object? sender, SetSceneEventArgs args)
    {
        _logger.Debug($"Fired for {args.Scene.DeclName}");

        var command = new SetSceneCommand(
            args.Scene.DeclName,
            () =>
            {
                _bdsmDriver.Interpreter.SetSceneVariable(args.Scene.DeclName);
            }
        );
        _commandManager.DispatchCommands(args.ExecutionContext, command);
    }

    public void ExitScene(SceneExit exit)
    {
        // Do we have anything to exit?
        if (_sceneManager.CurrentScene is null || _entityManager.Player is null)
        {
            _logger.Warning($"Cannot exit: {_entityManager.Player} : {_sceneManager.CurrentScene}");
            return;
        }

        // Can we exit?
        bool isAbleToExit = _bdsmDriver.Interpreter
            .IsAbleToExit(_sceneManager.CurrentScene.Name, exit.StartIndex);

        if (!isAbleToExit)
        {
            _logger.Debug($"Exit {exit.StartIndex} is disabled.");
            return;
        }

        _logger.Debug($"Exit {exit.StartIndex} -> {exit.Destination} ({exit.TargetIndex}).");
        // Stop whatever we're doing.
        _commandManager.Interrupt(interruptAsync: false);

        // Update scene.
        _sceneManager.ExitScene(_entityManager.Player, exit);

        // Fire onEnter() if available
        _bdsmDriver.Interpreter.EnterScene(_sceneManager.CurrentScene.Name);

        // Set the __scene var
        _bdsmDriver.Interpreter.SetSceneVariable(_sceneManager.CurrentScene.Name);
    }

    protected override void OnSetPlayer(object? sender, SetPlayerEventArgs args)
    {
        // TODO: Must be SetPlayerCommand!
        var player = _entityManager.Get<Actor>(args.Who.DeclName);
        _entityManager.Player = player;
    }

    protected override void OnDefineScene(object? sender, DefineSceneEventArgs args)
    {
        _logger.Debug($"Fired for {args.Scene.DeclName}");

        var scene = new Scene(args.Scene.DeclName);
        var musicName = (string)args.Scene.Get("music");
        scene.FriendlyName = (string)args.Scene.Get("name");
        if (!string.IsNullOrWhiteSpace(musicName))
        {
            scene.SetMusic(musicName);
        }

        foreach (var prop in scene.Props)
        {
            PropStatement propDefinition;
            try
            {
                propDefinition = args.Scene.Props.Single(p => p.declName.Lexeme == prop.DeclName);
            }
            catch
            {
                _logger.Error($"Missing prop definition for {prop.DeclName}.");
                throw;
            }
            prop.Name = (string)propDefinition.name.Literal!;


            // Use is a bit special as it requires an item
            // and there can be multiple definitions for multiple items.
            // So we process it below.
            foreach (var verb in Enum.GetValues<Verb>())
            {
                if (verb == Verb.Use)
                {
                    continue;
                }

                var verbDefinition = propDefinition.verbs.SingleOrDefault(vd =>
                    string.Equals(
                        vd.name.Lexeme,
                        verb.ToString(),
                        StringComparison.OrdinalIgnoreCase)
                );

                prop.VerbHandlers[verb] += (_, verbArgs) =>
                {
                    if (verbDefinition is null)
                    {
                        _bdsmDriver.Interpreter.CallDefaultVerbHandler(verb.ToString());
                    }
                    else
                    {
                        _bdsmDriver.Interpreter.Interpret(verbDefinition.statements);
                    }
                };
            }

            var useDefinitions = propDefinition.verbs.Where(vd =>
                string.Equals(
                    vd.name.Lexeme,
                    Verb.Use.ToString(),
                    StringComparison.OrdinalIgnoreCase)
            );
            prop.VerbHandlers[Verb.Use] += (_, verbArgs) =>
            {
                var isFoundMatch = false;
                foreach (var useDefinition in useDefinitions)
                {
                    if (string.Equals(
                        (verbArgs as UseVerbEventArgs)?.Item.DeclName,
                        useDefinition.item.Lexeme,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        _bdsmDriver.Interpreter.Interpret(useDefinition.statements);
                        isFoundMatch = true;
                    }
                }
                if (!isFoundMatch)
                {
                    _bdsmDriver.Interpreter.CallDefaultVerbHandler(Verb.Use.ToString());
                }
            };
        }

        _sceneManager.AddScene(scene);
    }

    protected override void OnDefineActor(object? sender, DefineActorEventArgs args)
    {
        var actor = new Actor(args.Who.DeclName);
        var colorstr = (string)args.Who.Get("color");
        actor.SpeechColor = ColorEx.FromHexString(colorstr);
        actor.Name = (string)args.Who.Get("name");
        actor.DeclName = args.Who.DeclName;

        // Use is a bit special as it requires an item
        // and there can be multiple definitions for multiple items.
        // So we process it below.
        foreach (var verb in Enum.GetValues<Verb>())
        {
            if (verb == Verb.Use)
            {
                continue;
            }

            var verbDefinition = args.Who.Verbs.SingleOrDefault(vd =>
                string.Equals(
                    vd.name.Lexeme,
                    verb.ToString(),
                    StringComparison.OrdinalIgnoreCase)
            );

            actor.VerbHandlers[verb] += (_, verbArgs) =>
            {
                if (verbDefinition is null)
                {
                    _bdsmDriver.Interpreter.CallDefaultVerbHandler(verb.ToString());
                }
                else
                {
                    _bdsmDriver.Interpreter.Interpret(verbDefinition.statements);
                }
            };
        }

        var useDefinitions = args.Who.Verbs.Where(vd =>
            string.Equals(
                vd.name.Lexeme,
                Verb.Use.ToString(),
                StringComparison.OrdinalIgnoreCase)
        );
        actor.VerbHandlers[Verb.Use] += (_, verbArgs) =>
        {
            var isFoundMatch = false;
            foreach (var useDefinition in useDefinitions)
            {
                if (string.Equals(
                    (verbArgs as UseVerbEventArgs)?.Item.DeclName,
                    useDefinition.item.Lexeme,
                    StringComparison.OrdinalIgnoreCase))
                {
                    _bdsmDriver.Interpreter.Interpret(useDefinition.statements);
                    isFoundMatch = true;
                }
            }
            if (!isFoundMatch)
            {
                _bdsmDriver.Interpreter.CallDefaultVerbHandler(Verb.Use.ToString());
            }
        };

        actor.OnPickup += (_, __) =>
        {
            actor.Scene = null;
            if (actor.Parent != null)
            {
                actor.Parent.Children.Remove(actor);
                actor.Parent = null;
            }
        };

        _entityManager.Add(actor);
    }

    protected override void OnActorSay(object? sender, ActorSayEventArgs e)
    {
        var command = new ActorSayCommand(
            _entityManager.Get<Actor>(e.Who.DeclName),
            new Phrase(e.What, width: Constants.TextWidthActorSpeech)
        );

        _commandManager.DispatchCommands(e.ExecutionContext, command);
    }

    protected override void OnFreespeech(object? sender, FreespeechEventArgs e)
    {
        var command = new FreespeechCommand(
            new Phrase(e.Text, width: Constants.TextWidthActorSpeech),
            e.ActorName,
            ColorEx.FromHexString(e.Color),
            new Vector2(e.X, e.Y)
        );

        _commandManager.DispatchCommands(e.ExecutionContext, command);
    }

    protected override void OnNarrate(object? sender, NarrateEventArgs e)
    {
        _logger.Debug();

        var command = new NarrateCommand(
            new Phrase(e.Text, width: Constants.TextWidthNarration)
        );

        _commandManager.DispatchCommands(e.ExecutionContext, command);
    }

    protected override void OnPlayAnimation(object? sender, PlayAnimationEventArgs e)
    {
        var who = _entityManager.Get<Actor>(e.What.DeclName);
        _commandManager.DispatchCommands(
            e.ExecutionContext, new PlayAnimationCommand(who, e.Name)
        );
    }

    protected override void OnPlaySong(object? sender, PlaySongEventArgs args)
    {
        _commandManager.DispatchCommands(
            args.ExecutionContext, new PlaySongCommand(args.Name)
        );
    }

    protected override void OnPlaySound(object? sender, PlaySoundEventArgs args)
    {
        _commandManager.DispatchCommands(
            args.ExecutionContext, new PlaySoundCommand(args.Name)
        );
    }

    protected override void OnStopSong(object? sender, StopSongEventArgs args)
    {
        _commandManager.DispatchCommands(
            args.ExecutionContext, new StopSongCommand()
        );
    }

    protected override void OnPlayVideo(object? sender, PlayVideoEventArgs args)
    {
        _commandManager.DispatchCommands(
            args.ExecutionContext, new PlayVideoCommand(args.Name)
        );
    }

    protected override void OnRequestStartMenu(object? sender, RequestStartMenuEventArgs args)
    {
        _commandManager.DispatchCommands(
            args.ExecutionContext, new RequestStartMenuCommand()
        );
    }

    protected override void OnRequestMainMenu(object? sender, RequestMainMenuEventArgs args)
    {
        _commandManager.DispatchCommands(
            args.ExecutionContext, new RequestMainMenuCommand(args.IsStarted)
        );
    }

    protected override void OnRequestTutorial(object? sender, RequestTutorialEventArgs args)
    {
        _commandManager.DispatchCommands(
            args.ExecutionContext, new RequestTutorialCommand()
        );
    }

    protected override void OnItemPickup(object? sender, ItemPickupEventArgs e)
    {
        _logger.Debug($"Fired for {e.Who.DeclName}->{e.What.DeclName}");

        var who = _entityManager.Get<Actor>(e.Who.DeclName);
        var what = _entityManager.Get<Actor>(e.What.DeclName);

        List<Command> commands = [
            new MoveCommand(who, what.Position),
            new PickupCommand(who, what)
        ];

        _commandManager.DispatchCommands(e.ExecutionContext, [.. commands]);
    }

    protected override void OnPlayerItemRemove(object? sender, PlayerItemRemoveEventArgs e)
    {
        var who = _entityManager.Get<Actor>(e.Who.DeclName);
        var what = _entityManager.Get<Actor>(e.What.DeclName);

        _commandManager.DispatchCommands(e.ExecutionContext, new RemovePlayerItemCommand(who, what));
    }

    protected override void OnPlayerItemAdd(object? sender, PlayerItemAddEventArgs e)
    {
        var who = _entityManager.Get<Actor>(e.Who.DeclName);
        var what = _entityManager.Get<Actor>(e.What.DeclName);

        _commandManager.DispatchCommands(e.ExecutionContext, new AddPlayerItemCommand(who, what));
    }

    protected override void OnActorTalkTo(object? sender, ActorTalkToEventArgs e)
    {
        _logger.Debug($"Fired for {e.Who.DeclName}<->{e.To.DeclName}");

        var who = _entityManager.Get<Actor>(e.Who.DeclName);
        var to = _entityManager.Get<Actor>(e.To.DeclName);
        var index = e.NodeIndex;

        List<Command> commands = [];
        if (e.IsWalkTo)
        {
            commands.Add(new MoveCommand(who, to, stopDistance: 400f));
        }
        commands.Add(new TalkToCommand(who, to, index));

        _commandManager.DispatchCommands(e.ExecutionContext, commands.ToArray());
    }

    protected override void OnActorMove(object? sender, ActorMoveEventArgs args)
    {
        var who = _entityManager.Get<Actor>(args.Who.DeclName);
        _commandManager.DispatchCommands(args.ExecutionContext, new MoveCommand(who, new Vector2(args.X, args.Y)));
    }

    protected override void OnActorPut(object? sender, ActorPutEventArgs args)
    {
        var who = _entityManager.Get<Actor>(args.Who.DeclName);
        var what = _entityManager.Get<Actor>(args.What.DeclName);

        List<Command> commands =
        [
            // commands.Add(new MoveCommand(who, new Vector2(args.X, args.Y)));
            new ScenePutCommand(who, what, new Vector2(args.X, args.Y)),
        ];

        _commandManager.DispatchCommands(args.ExecutionContext, [.. commands]);
    }

    protected override void OnSetPos(object? sender, SetPosEventArgs args)
    {
        var actor = _entityManager.Get<Entity>(args.What.DeclName);

        _commandManager.DispatchCommands(
            args.ExecutionContext,
            new SetPositionCommand(actor, new Vector2(args.X, args.Y))
        );
    }

    protected override void OnSceneRemove(object? sender, SceneRemoveEventArgs args)
    {
        var scene = _sceneManager.GetScene(args.Scene.DeclName);
        var what = _entityManager.Get<Entity>(args.What.DeclName);

        _commandManager.DispatchCommands(args.ExecutionContext, new SceneRemoveCommand(what));
    }

    protected override void OnSceneAdd(object? sender, SceneAddEventArgs args)
    {
        var scene = _sceneManager.GetScene(args.Scene.DeclName);
        var what = _entityManager.Get<Entity>(args.What.DeclName);

        _commandManager.DispatchCommands(args.ExecutionContext, new SceneAddCommand(scene, what));
    }

    protected override void OnActorInterrupt(object? sender, ActorInterruptEventArgs e)
    {
        var who = _entityManager.Get<Actor>(e.ExecutionContext.ActorName);
        _commandManager.DispatchCommands(e.ExecutionContext, new ActorInterruptCommand(who));
    }

    protected override void OnInterrupt(object? sender, InterruptEventArgs e)
    {
        //_commandManager.DispatchCommands(e.ExecutionContext, new InterruptCommand());
        // TODO: This is a hack.
        // If this is in a block, and we queue this command, it also terminates 
        // the rest of the commands in this block.
        // To mitigate, we could introduce some tagging for queues and only erase
        // the ones before? Is this all an overkill anyway?
        new InterruptCommand(e.InterruptAsync).Startup();
    }

    protected override void OnWait(object? sender, WaitEventArgs e)
    {
        var command = new WaitCommand(e.TimeoutMillis);
        /*
         * TODO: Is this even a thing??
        if (e.ExecutionContext.ContextType == ExecutionContextType.Actor)
        {
            command.Actor = _entityManager.Get<Actor>(e.ExecutionContext.ActorName);
        }
        */
        _commandManager.DispatchCommands(e.ExecutionContext, command);
    }

    protected override void OnCamZoom(object? sender, CamZoomEventArgs args)
    {
        _commandManager.DispatchCommands(args.ExecutionContext, new CamZoomCommand(args.Zoom));
    }

    protected override void OnSetCamPos(object? sender, SetCamPosEventArgs args)
    {
        _commandManager.DispatchCommands(args.ExecutionContext, new SetCamPosCommand(args.X, args.Y));
    }

    protected override void OnFadeIn(object? sender, FadeInEventArgs args)
    {
        _commandManager.DispatchCommands(args.ExecutionContext, new FadeInCommand(args.Speed));
    }
    protected override void OnFadeOut(object? sender, FadeOutEventArgs args)
    {
        _commandManager.DispatchCommands(args.ExecutionContext, new FadeOutCommand(args.Speed));
    }

    protected override void OnCloseup(object? sender, CloseupEventArgs args)
    {
        _commandManager.DispatchCommands(args.ExecutionContext, new CloseupCommand(args.Name));
    }

    protected override void OnCinematicStart(object? sender, CinematicStartEventArgs args)
    {
        _commandManager.DispatchCommands(ExecutionContext.Shared, new CinematicStartCommand());
    }

    protected override void OnCinematicEnd(object? sender, CinematicEndEventArgs args)
    {
        _commandManager.DispatchCommands(ExecutionContext.Shared, new CinematicEndCommand());
    }

    protected override void OnEndGame(object? sender, EndGameEventArgs args)
    {
        _commandManager.DispatchCommands(args.ExecutionContext, new EndGameCommand());
    }

    protected override void OnTip(object? sender, TipEventArgs args)
    {
        _commandManager.DispatchCommands(
            args.ExecutionContext,
            new TipCommand(args.DurationSeconds, args.Id, args.Text)
        );
    }

    protected override void OnNotify(object? sender, NotifyEventArgs e)
    {
        _commandManager.DispatchCommands(
            e.ExecutionContext,
            new NotifyCommand(e.DurationSeconds, e.Text)
        );
    }

    public bool LaunchGame(string? customLaunchScriptPath = null)
    {
        _bdsmDriver.Interpreter.RestartEnvironment();

        var scriptOk = customLaunchScriptPath is not null
            ? _bdsmDriver.RunFileFromAbsolutePath(customLaunchScriptPath)
            : _bdsmDriver.RunFileFromRelativePath("launch.bs");
        if (!scriptOk)
        {
            _logger.Error("Failed to load script...");
            return false;
        }
        return true;
    }

    public bool ReinitGame()
    {
        _commandManager.Interrupt(interruptAsync: true);
        _bdsmDriver.Interpreter.RestartEnvironment();
        _historyService.Clear();
        _musicPlayerService.StopImmediately();

        var scriptOk = _bdsmDriver.RunFileFromRelativePath("imports.bs");
        if (!scriptOk)
        {
            _logger.Error("Failed to load script...");
            return false;
        }
        return true;
    }

    public bool NewGame()
    {
        ReinitGame();

        var script = "newgame.bs";
        var scriptOk = _bdsmDriver.RunFileFromRelativePath(script);
        if (!scriptOk)
        {
            _logger.Error("Failed to load script...");
            return false;
        }

        return true;
    }

    protected override void OnDetachCam(object? sender, DetachCamEventArgs e)
    {
        _commandManager.DispatchCommands(e.ExecutionContext, new DetachCamCommand());
    }

    protected override void OnRequestStatePlaying(object? sender, RequestStatePlayingEventArgs e)
    {
        _commandManager.DispatchCommands(e.ExecutionContext, new RequestStatePlayingCommand());
    }

    protected override void OnSetState(object? sender, SetStateEventArgs e)
    {
        var who = _entityManager.Get<Actor>(e.Actor.DeclName);
        var command = new SetStateCommand(who, e.State);
        command.Completed += (_, __) =>
        {
            e.Actor.State = e.State;
        };
        _commandManager.DispatchCommands(e.ExecutionContext, command);
    }

    protected override void OnActorSetIsShowChildren(object? sender, ActorSetIsShowChildrenEventArgs e)
    {
        var who = _entityManager.Get<Actor>(e.Actor.DeclName);
        _commandManager.DispatchCommands(e.ExecutionContext, new ActorSetIsShowChildrenCommand(who, e.IsShowChildren));
    }

    protected override void OnMoveCamTo(object? sender, MoveCamToEventArgs e)
    {
        _commandManager.DispatchCommands(e.ExecutionContext, new MoveCamToCommand(e.X, e.Y, e.Speed));
    }

    public void OnRegionEntered(Scene currentScene, SceneRegion region)
    {
        // TODO: Times entered.
        _bdsmDriver.Interpreter.EnterSceneRegion(currentScene.Name, region.Name);
    }

    public void OnRegionExited(Scene currentScene, SceneRegion region)
    {
        _bdsmDriver.Interpreter.ExitSceneRegion(currentScene.Name, region.Name);
    }

    protected override void OnSetCostume(object? sender, SetCostumeEventArgs e)
    {
        var who = _entityManager.Get<Actor>(e.Actor.DeclName);
        _commandManager.DispatchCommands(e.ExecutionContext, new SetCostumeCommand(who, e.Name));
    }

    protected override void OnAutosave(object? sender, AutosaveEventArgs e)
    {
        _commandManager.DispatchCommands(e.ExecutionContext, new AutosaveCommand());
    }

    protected override void OnClearInventory(object? sender, ClearInventoryEventArgs e)
    {
        var who = _entityManager.Get<Actor>(e.Who.DeclName);
        _commandManager.DispatchCommands(e.ExecutionContext, new ClearInventoryCommand(who));
    }
}