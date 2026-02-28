using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Beta.DI;
using System;
using Beta.Gui;
using aced.Controllers;
using aced.Models;
using Beta.Entities.Animations;
using aced.Editors;
using Beta.Gui.Elements;
using System.Globalization;
using Beta.Input;
using System.Collections.Generic;

namespace aced;

public class EditorManager : IInputEventListener
{
    public EditorState EditorState { get; set; } = EditorState.None;
    public ActorEditor ActorEditor { get; }
    public SceneEditor SceneEditor { get; }

    public EventHandler OnExit { get; set; } = (_, _) => { };

    private GuiElement _leftPane;
    private GuiElement _rightPane;
    private GuiElement _rightBottomPane;
    private readonly InputContextManager _inputContextManager;
    private readonly InputService _input;
    private readonly InputContext _guiContext;
    private readonly InputContext _actorEditorContext;
    private readonly InputContext _sceneEditorContext;

    public EditorManager()
    {
        ActorEditor = DependencyContainer.Instance.Get<ActorEditor>();
        SceneEditor = DependencyContainer.Instance.Get<SceneEditor>();
        SceneEditor.SceneActorPositionChanged += OnSceneActorPositionChanged;
        SceneEditor.SceneActorScaleChanged += OnSceneActorScaleChanged;
        SceneEditor.SceneActorSelectedOnScene += (_, actor) =>
        {
            RequestSceneActorProps(actor);
        };

        SceneEditor.SceneLightPositionChanged += OnSceneLightPositionChanged;
        SceneEditor.SceneLightSelectedOnScene += (_, light) =>
        {
            RequestSceneLightProps(light);
        };

        _inputContextManager = DependencyContainer.Instance.Get<InputContextManager>();
        _input = DependencyContainer.Instance.Get<InputService>();

        _input.AddListener(ActorEditor);
        _input.AddListener(SceneEditor);

        _guiContext = _inputContextManager.GetOrCreateByName(nameof(Gui));
        _actorEditorContext = _inputContextManager.GetOrCreateByName(nameof(ActorEditor));
        _sceneEditorContext = _inputContextManager.GetOrCreateByName(nameof(SceneEditor));
    }

    public void RequestActorEditor()
    {
        DestroyPanes();

        _leftPane = Gui.Instance.AppendToRoot(
            "Layouts/actor-leftpane.xml",
            "Layouts/actor-leftpane.css"
        );

        _leftPane.TryFindById<TextInput>("actor-name-input", out var nameInput);
        _leftPane.TryFindById<TextInput>("actor-speed-input", out var speedInput);
        nameInput.Text = ActorEditor.CurrentActorData.Name;
        speedInput.Text = ActorEditor.CurrentActorData.Speed.ToString(CultureInfo.InvariantCulture);
        foreach (var costume in ActorEditor.Costumes)
        {
            ActorEditorController.Instance.AddCostumeToActorCostumeList(costume);
        }

        EditorState = EditorState.ActorEdit;
    }

    public void RequestSceneEditor()
    {
        DestroyPanes();

        _leftPane = Gui.Instance.AppendToRoot(
            "Layouts/scene-leftpane.xml",
            "Layouts/scene-leftpane.css"
        );

        _leftPane.TryFindById<TextInput>("scene-name-input", out var nameInput);
        nameInput.Text = SceneEditor.CurrentScene.Name;

        foreach (var exit in SceneEditor.CurrentScene.Exits)
        {
            SceneEditorController.Instance.AddExitToSceneExitList(exit);
        }
        foreach (var actor in SceneEditor.CurrentScene.Actors)
        {
            SceneEditorController.Instance.AddActorToSceneActorList(actor);
        }
        foreach (var region in SceneEditor.CurrentScene.Regions)
        {
            SceneEditorController.Instance.AddRegionToSceneRegionList(region);
        }
        foreach (var prop in SceneEditor.CurrentScene.Props)
        {
            SceneEditorController.Instance.AddPropToScenePropList(prop);
        }
        foreach (var walkbehind in SceneEditor.CurrentScene.Walkbehinds)
        {
            SceneEditorController.Instance.AddWalkbehindToSceneWalkbehindsList(walkbehind);
        }
        foreach (var light in SceneEditor.CurrentScene.Lights)
        {
            SceneEditorController.Instance.AddLightToSceneLightsList(light);
        }

        EditorState = EditorState.SceneEdit;
    }

    public void RequestCostumeProps(Costume c)
    {
        DestroyRightPane();

        _rightPane = Gui.Instance.AppendToRoot(
            "Layouts/costume-props.xml",
            "Layouts/costume-props.css"
        );

        _rightPane.TryFindById<TextInput>("costume-name-input", out var nameInput);

        nameInput.Text = c.Name;

        foreach (var anim in c.Animations)
        {
            ActorEditorController.Instance.AddAnimationToCostumeAnimationList(anim);
        }
    }

    public void RequestAnimationProps(Animation anim)
    {
        DestroyRightBottomPane();

        _rightBottomPane = Gui.Instance.LoadFromFiles(
            "Layouts/animation-props.xml",
            "Layouts/animation-props.css",
            _rightPane.Style.LayerDepth
        );
        _rightPane.AddElement(_rightBottomPane);

        _rightBottomPane.TryFindById<TextInput>("animation-name-input", out var nameInput);
        _rightBottomPane.TryFindById<TextInput>("animation-speed-input", out var speedInput);
        _rightBottomPane.TryFindById<Checkbox>("animation-repeat-checkbox", out var repeatInput);

        nameInput.Text = anim.Name;
        speedInput.Text = anim.Speed.ToString(CultureInfo.InvariantCulture);
        repeatInput.Value = anim.Repeat;
    }


    public void RequestScaleMapProps()
    {
        DestroyRightPane();

        _rightPane = Gui.Instance.AppendToRoot(
            "Layouts/scene-scalemap-props.xml",
            "Layouts/scene-scalemap-props.css"
        );

        _rightPane.TryFindById<TextInput>("scalemap-minscale-input", out var minScaleInput);

        minScaleInput.Text = SceneEditor.CurrentScene.ScaleMap.MinScale.ToString(CultureInfo.InvariantCulture);
    }
    public void RequestExitProps(SceneExit exit)
    {
        DestroyRightPane();

        _rightPane = Gui.Instance.AppendToRoot(
            "Layouts/exit-props.xml",
            "Layouts/exit-props.css"
        );

        _rightPane.TryFindById<TextInput>("exit-destination-input", out var destInput);
        _rightPane.TryFindById<TextInput>("exit-index-input", out var indexInput);
        _rightPane.TryFindById<TextInput>("exit-target-index-input", out var targetIndexInput);

        destInput.Text = exit.Destination;
        indexInput.Text = exit.Index < 0 ? "" : exit.Index.ToString(CultureInfo.InvariantCulture);
        targetIndexInput.Text = exit.TargetIndex < 0 ? "" : exit.TargetIndex.ToString(CultureInfo.InvariantCulture);
    }

    public void RequestScenePropProps(SceneProp prop)
    {
        DestroyRightPane();

        _rightPane = Gui.Instance.AppendToRoot(
            "Layouts/prop-props.xml",
            "Layouts/prop-props.css"
        );

        _rightPane.TryFindById<TextInput>("prop-name-input", out var propNameInput);

        propNameInput.Text = prop.Name;
    }

    public void RequestSceneWalkbehindProps(SceneWalkbehind walkbehind)
    {
        DestroyRightPane();

        _rightPane = Gui.Instance.AppendToRoot(
            "Layouts/walkbehind-props.xml",
            "Layouts/walkbehind-props.css"
        );

        _rightPane.TryFindById<TextInput>("walkbehind-name-input", out var walkbehindNameInput);

        walkbehindNameInput.Text = walkbehind.Name;
    }

    public void RequestSceneRegionProps(SceneRegion region)
    {
        DestroyRightPane();

        _rightPane = Gui.Instance.AppendToRoot(
            "Layouts/region-props.xml",
            "Layouts/region-props.css"
        );

        _rightPane.TryFindById<TextInput>("region-name-input", out var regionNameInput);
        // Do we need this?
        _rightPane.TryFindById<TextInput>("region-times-active-input", out var timesActiveInput);

        regionNameInput.Text = region.Name;
        timesActiveInput.Text = region.TimesActive.ToString(CultureInfo.InvariantCulture);
    }
    public void RequestSceneActorProps(SceneActor actor)
    {
        DestroyRightPane();

        _rightPane = Gui.Instance.AppendToRoot(
            "Layouts/scene-actor-props.xml",
            "Layouts/scene-actor-props.css"
        );

        _rightPane.TryFindById<Label>("scene-actor-props-label", out var sceneActorPropsLabel);
        _rightPane.TryFindById<TextInput>("scene-actor-position-input", out var sceneActorPositionInput);
        _rightPane.TryFindById<TextInput>("scene-actor-scale-input", out var sceneActorScaleInput);
        _rightPane.TryFindById<TextInput>("scene-actor-state-input", out var sceneActorStateInput);
        _rightPane.TryFindById<Checkbox>("scene-actor-show-children-checkbox", out var sceneActorShowChildrenCheckbox);
        _rightPane.TryFindById<ListView<GuiElement>>("scene-actor-children-list", out var sceneActorChildrenList);

        sceneActorPropsLabel.Text = $"{actor.Name} Properties";
        sceneActorPositionInput.Text = $"{(int)actor.Position.X}, {(int)actor.Position.Y}";
        sceneActorScaleInput.Text = actor.Scale.ToString(CultureInfo.InvariantCulture);
        sceneActorStateInput.Text = actor.State;
        sceneActorShowChildrenCheckbox.Value = actor.IsShowChildren;
        foreach (var child in actor.Children)
        {
            SceneEditorController.Instance.AddActorToActorChildrenList(child);
        }
    }

    private void OnSceneActorPositionChanged(object sender, Vector2 e)
    {
        _rightPane.TryFindById<TextInput>("scene-actor-position-input", out var sceneActorPositionInput);
        sceneActorPositionInput.Text = $"{e.X}, {e.Y}";
    }
    private void OnSceneActorScaleChanged(object sender, float e)
    {
        _rightPane.TryFindById<TextInput>("scene-actor-scale-input", out var sceneActorScaleInput);
        sceneActorScaleInput.Text = e.ToString(CultureInfo.InvariantCulture);
    }

    private void OnSceneLightPositionChanged(object sender, Vector3Model e)
    {
        _rightPane.TryFindById<TextInput>("light-x-input", out var xInput);
        _rightPane.TryFindById<TextInput>("light-y-input", out var yInput);
        _rightPane.TryFindById<TextInput>("light-z-input", out var zInput);

        xInput.Text = e.X.ToString(CultureInfo.InvariantCulture);
        yInput.Text = e.Y.ToString(CultureInfo.InvariantCulture);
        zInput.Text = e.Z.ToString(CultureInfo.InvariantCulture);
    }

    public void RequestSceneLightProps(SceneLight light)
    {
        DestroyRightPane();

        _rightPane = Gui.Instance.AppendToRoot(
            "Layouts/light-props.xml",
            "Layouts/light-props.css"
        );

        _rightPane.TryFindById<TextInput>("light-x-input", out var lightXInput);
        _rightPane.TryFindById<TextInput>("light-y-input", out var lightYInput);
        _rightPane.TryFindById<TextInput>("light-z-input", out var lightZInput);
        _rightPane.TryFindById<TextInput>("light-color-input", out var lightColorInput);

        lightXInput.Text = light.LightPosition.X.ToString(CultureInfo.InvariantCulture);
        lightYInput.Text = light.LightPosition.Y.ToString(CultureInfo.InvariantCulture);
        lightZInput.Text = light.LightPosition.Z.ToString(CultureInfo.InvariantCulture);
        lightColorInput.Text = light.LightColorHex;
    }


    private void DestroyLeftPane()
    {
        if (_leftPane is not null)
        {
            Gui.Instance.RemoveFromRoot(_leftPane);
            _leftPane = null;
        }
    }

    public void DestroyRightPane()
    {
        if (_rightPane is not null)
        {
            Gui.Instance.RemoveFromRoot(_rightPane);
            _rightPane = null;
            _rightBottomPane = null;
        }
    }

    public void DestroyRightBottomPane()
    {
        if (_rightBottomPane is not null)
        {
            _rightPane.RemoveElement(_rightBottomPane);
            _rightBottomPane = null;
        }
    }

    public void RequestExit()
    {
        OnExit(this, null);
    }

    private void DestroyPanes()
    {
        DestroyLeftPane();
        DestroyRightPane();
    }

    public void Update(GameTime gameTime)
    {
        switch (EditorState)
        {
            case EditorState.ActorEdit:
                ActorEditor.Update(gameTime);
                break;
            case EditorState.SceneEdit:
                SceneEditor.Update(gameTime);
                break;
            case EditorState.None:
            default:
                break;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        switch (EditorState)
        {
            case EditorState.ActorEdit:
                ActorEditor.Draw(spriteBatch);
                break;
            case EditorState.SceneEdit:
                SceneEditor.Draw(spriteBatch);
                break;
            case EditorState.None:
            default:
                break;
        }
    }

    public HashSet<InputContext> GetInputContexts()
    {
        return [
            _guiContext,
            _actorEditorContext,
            _sceneEditorContext,
        ];
    }

    public InputEventConsumeResult OnInputEvent(InputEventArgs args)
    {
        if (args.EventType == InputEventType.MouseMoved)
        {
            switch(EditorState)
            {
                case EditorState.SceneEdit:
                    if (SceneEditor.Viewport.Contains(args.GetCursorPosition()))
                    {
                        _input.CurrentContext = _sceneEditorContext;
                    }
                    else
                    {
                        _input.CurrentContext = _guiContext;
                    }
                    break;
                case EditorState.ActorEdit:
                    if (ActorEditor.Viewport.Contains(args.GetCursorPosition()))
                    {
                        _input.CurrentContext = _actorEditorContext;
                    }
                    else
                    {
                        _input.CurrentContext = _guiContext;
                    }
                    break;
                case EditorState.None:
                default:
                    break;
            }
        }

        return new();
    }
}