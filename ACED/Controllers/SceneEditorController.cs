using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using aced.Models;
using Beta.DI;
using Beta.Gui;
using Beta.Gui.Elements;
using Beta.Gui.Events;
using Beta.Logging;

namespace aced.Controllers;

internal class SceneEditorController : GuiHandler<SceneEditorController>
{
    private readonly EditorManager _editorManager;
    private readonly Dictionary<SceneExit, Label> _exitLabelDict = [];
    private readonly Dictionary<SceneActor, Label> _actorLabelDict = [];
    private readonly Dictionary<SceneRegion, Label> _regionLabelDict = [];
    private readonly Dictionary<SceneProp, Label> _propLabelDict = [];
    private readonly Dictionary<SceneLight, Label> _lightLabelDict = [];
    private readonly Dictionary<SceneWalkbehind, Label> _walkbehindLabelDict = [];
    private readonly ILogger _logger;

    public SceneEditorController()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _editorManager = DependencyContainer.Instance.Get<EditorManager>();
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "scene-name-input")]
    public void HandleSceneNameInput(string value)
    {
        _editorManager.SceneEditor.ChangeSceneName(value);
    }

    [HandlerFor(GuiEventType.LeftClick, "scene-texture-button")]
    public void HandleSceneTextureButton()
    {
        var filename = UserRequestTextureFile();
        if (string.IsNullOrWhiteSpace(filename))
        {
            _logger.Info("No scene texture selected.");
            return;
        }
        _editorManager.SceneEditor.SetSceneTexture(filename);
    }

    [HandlerFor(GuiEventType.LeftClick, "scene-walkables-button")]
    public void HandleSceneWalkablesButton()
    {
        _editorManager.SceneEditor.EditWalkableAreas();
    }

    [HandlerFor(GuiEventType.LeftClick, "scene-scalemap-button")]
    public void HandleSceneScalemapButton()
    {
        _editorManager.SceneEditor.EditScaleMap();
        _editorManager.RequestScaleMapProps();
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "scalemap-minscale-input")]
    public void HandleScalemapMinscaleInput(string value)
    {
        _editorManager.SceneEditor.SetScaleMapMinScale(float.Parse(value, CultureInfo.InvariantCulture));
    }

    [HandlerFor(GuiEventType.LeftClick, "scene-add-exit-button")]
    public void HandleSceneAddExitButton()
    {
        var exit = _editorManager.SceneEditor.AddExit();
        _editorManager.SceneEditor.SetCurrentExit(exit);

        AddExitToSceneExitList(exit);

        _editorManager.RequestExitProps(exit);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "exit-destination-input")]
    public void HandleExitDestinationInput(string value)
    {
        _editorManager.SceneEditor.SelectedExit.Destination = value;
        _exitLabelDict[_editorManager.SceneEditor.SelectedExit].Text = value;
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "exit-index-input")]
    public void HandleExitIndexInput(string value)
    {
        _editorManager.SceneEditor.SelectedExit.Index = int.Parse(value, CultureInfo.InvariantCulture);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "exit-target-index-input")]
    public void HandleExitTargetIndexInput(string value)
    {
        _editorManager.SceneEditor.SelectedExit.TargetIndex = int.Parse(value, CultureInfo.InvariantCulture);
    }

    //
    // Actors
    [HandlerFor(GuiEventType.LeftClick, "scene-add-actor-button")]
    public void HandleSceneAddActorButton()
    {
        var filename = UserRequestActorFile();
        if (string.IsNullOrWhiteSpace(filename))
        {
            _logger.Info("No actor file selected.");
            return;
        }
        var actor = _editorManager.SceneEditor.AddActorToScene(filename);
        _editorManager.SceneEditor.SetCurrentActor(actor);

        AddActorToSceneActorList(actor);

        _editorManager.RequestSceneActorProps(actor);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "scene-actor-position-input")]
    public void HandleSceneActorPositionInput(string value)
    {
        var posStr = value.Split(',');
        var x = int.Parse(posStr[0], CultureInfo.InvariantCulture);
        var y = int.Parse(posStr[1], CultureInfo.InvariantCulture);
        _editorManager.SceneEditor.SelectedActor.Position = new Coord(x, y);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "scene-actor-scale-input")]
    public void HandleSceneActorScaleInput(string value)
    {
        _editorManager.SceneEditor.SelectedActor.Scale = float.Parse(value, CultureInfo.InvariantCulture);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "scene-actor-costume-input")]
    public void HandleSceneActorCostumeInput(string value)
    {
        _editorManager.SceneEditor.SetSelectedActorCostume(value);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "scene-actor-state-input")]
    public void HandleSceneActorStateInput(string value)
    {
        _editorManager.SceneEditor.SetSelectedActorState(value);
    }

    [HandlerFor(GuiEventType.CheckboxToggle, "scene-actor-show-children-checkbox")]
    public void HandleSceneActorShowChildrenCheckbox(bool value)
    {
        _editorManager.SceneEditor.SelectedActor.IsShowChildren = value;
    }

    [HandlerFor(GuiEventType.LeftClick, "scene-actor-add-child-button")]
    public void HandleSceneActorAddChildButton()
    {
        var filename = UserRequestActorFile();
        if (string.IsNullOrWhiteSpace(filename))
        {
            _logger.Info("No actor file selected.");
            return;
        }

        var actor = _editorManager.SceneEditor.AddChildActorToSelectedActor(filename);
        AddActorToActorChildrenList(actor);
    }

    [HandlerFor(GuiEventType.LeftClick, "scene-actor-edit-script-button")]
    public void HandleSceneActorEditScriptButton()
    {
        var actorName = _editorManager.SceneEditor.SelectedActor.Name;
        var launchScriptPath
            = $"{Settings.SCRIPTS_BASE_PATH}/actors/{actorName}.bs";

        var start = new ProcessStartInfo
        {
            FileName = "subl",
            Arguments = launchScriptPath
        };
        var proc = Process.Start(start);
    }
    [HandlerFor(GuiEventType.LeftClick, "scene-actor-edit-dialogue-button")]
    public void HandleSceneActorEditDialogueButton()
    {
        var actorName = _editorManager.SceneEditor.SelectedActor.Name;

        // If it doesn't exist, create a dialogue file.
        var dialoguePath =
            $"{Settings.CONTENT_BASE_PATH}/actors/{actorName}/{actorName}.dialogue.json";
        if (!File.Exists(dialoguePath))
        {
            var template = File.ReadAllText("templates/template_actor_dialogue.txt");
            File.WriteAllText(dialoguePath, template);
        }

        var start = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = Settings.AiDS_START,
            WorkingDirectory = Settings.AiDS_PWD
        };
        var proc = Process.Start(start);
    }


    //
    // Regions
    [HandlerFor(GuiEventType.LeftClick, "scene-add-region-button")]
    public void HandleSceneAddRegionButton()
    {
        var region = _editorManager.SceneEditor.AddRegion();
        _editorManager.SceneEditor.SetCurrentRegion(region);

        AddRegionToSceneRegionList(region);

        _editorManager.RequestSceneRegionProps(region);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "region-name-input")]
    public void HandleRegionNameInput(string value)
    {
        _editorManager.SceneEditor.SelectedRegion.Name = value;
        _regionLabelDict[_editorManager.SceneEditor.SelectedRegion].Text = value;
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "region-times-active-input")]
    public void HandleRegionTimesActiveInput(string value)
    {
        _editorManager.SceneEditor.SelectedRegion.TimesActive = int.Parse(value, CultureInfo.InvariantCulture);
    }


    // Props
    [HandlerFor(GuiEventType.LeftClick, "scene-add-prop-button")]
    public void HandleSceneAddPropButton()
    {
        var prop = _editorManager.SceneEditor.AddProp();
        _editorManager.SceneEditor.SetCurrentProp(prop);

        AddPropToScenePropList(prop);

        _editorManager.RequestScenePropProps(prop);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "prop-name-input")]
    public void HandlePropNameInput(string value)
    {
        _editorManager.SceneEditor.SelectedProp.Name = value;
        _propLabelDict[_editorManager.SceneEditor.SelectedProp].Text = value;
    }

    // Walkbehinds
    [HandlerFor(GuiEventType.LeftClick, "scene-add-walkbehind-button")]
    public void HandleSceneAddWalkbehindButton()
    {
        var walkbehind = _editorManager.SceneEditor.AddWalkbehind();
        _editorManager.SceneEditor.SetCurrentWalkbehind(walkbehind);

        AddWalkbehindToSceneWalkbehindsList(walkbehind);

        _editorManager.RequestSceneWalkbehindProps(walkbehind);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "walkbehind-name-input")]
    public void HandleWalkbehindNameInput(string value)
    {
        _editorManager.SceneEditor.SelectedWalkbehind.Name = value;
        _walkbehindLabelDict[_editorManager.SceneEditor.SelectedWalkbehind].Text = value;
    }

    // Lights
    [HandlerFor(GuiEventType.LeftClick, "scene-add-light-button")]
    public void HandleSceneAddLightButton()
    {
        var light = _editorManager.SceneEditor.AddLight();
        _editorManager.SceneEditor.SetCurrentLight(light);

        AddLightToSceneLightsList(light);

        _editorManager.RequestSceneLightProps(light);
    }
    [HandlerFor(GuiEventType.TextInputValueChanged, "light-x-input")]
    public void HandleLightXInput(string value)
    {
        _editorManager.SceneEditor.SelectedLight.LightPosition.X
            = float.Parse(value, CultureInfo.InvariantCulture);
    }
    [HandlerFor(GuiEventType.TextInputValueChanged, "light-y-input")]
    public void HandleLightYInput(string value)
    {
        _editorManager.SceneEditor.SelectedLight.LightPosition.Y
            = float.Parse(value, CultureInfo.InvariantCulture);
    }
    [HandlerFor(GuiEventType.TextInputValueChanged, "light-z-input")]
    public void HandleLightZInput(string value)
    {
        _editorManager.SceneEditor.SelectedLight.LightPosition.Z
            = float.Parse(value, CultureInfo.InvariantCulture);
    }
    [HandlerFor(GuiEventType.TextInputValueChanged, "light-color-input")]
    public void HandleLightColorInput(string value)
    {
        _editorManager.SceneEditor.SelectedLight.LightColorHex = value;
    }

    public void AddExitToSceneExitList(SceneExit exit)
    {
        Gui.Instance.TryFindById<ListView<GuiElement>>("scene-exits-list", out var list);

        Action onSelect = () =>
        {
            _editorManager.RequestExitProps(exit);
            _editorManager.SceneEditor.SetCurrentExit(exit);
        };
        Action onDelete = () =>
        {
            if (exit == _editorManager.SceneEditor.SelectedExit)
            {
                _editorManager.DestroyRightPane();
            }
            _editorManager.SceneEditor.DeleteExit(exit);
        };
        ControllerBase.AddEntityListItem(
            exit,
            list,
            _exitLabelDict,
            onSelect,
            onDelete,
            labelTextSelector: e => e.Destination
        );
    }

    public void AddActorToSceneActorList(SceneActor actor)
    {
        Gui.Instance.TryFindById<ListView<GuiElement>>("scene-actors-list", out var list);

        Action onSelect = () =>
        {
            _editorManager.RequestSceneActorProps(actor);
            _editorManager.SceneEditor.SetCurrentActor(actor);
        };
        Action onDelete = () =>
        {
            if (_editorManager.SceneEditor.IsEditingGivenActor(actor))
            {
                _editorManager.DestroyRightPane();
            }
            _editorManager.SceneEditor.DeleteActor(actor);
        };
        ControllerBase.AddEntityListItem(
            actor,
            list,
            _actorLabelDict,
            onSelect,
            onDelete,
            labelTextSelector: a => a.Name
        );
    }

    public void AddActorToActorChildrenList(SceneActor actor)
    {
        Gui.Instance.TryFindById<ListView<GuiElement>>("scene-actor-children-list", out var list);

        Action onSelect = () =>
        {
            _editorManager.SceneEditor.SetCurrentActor(actor);
        };
        Action onDelete = () =>
        {
            _editorManager.SceneEditor.DeleteChildActor(actor);
        };
        ControllerBase.AddEntityListItem(
            actor,
            list,
            _actorLabelDict,
            onSelect,
            onDelete,
            labelTextSelector: a => a.Name
        );
    }

    public void AddRegionToSceneRegionList(SceneRegion region)
    {
        Gui.Instance.TryFindById<ListView<GuiElement>>("scene-regions-list", out var list);

        Action onSelect = () =>
        {
            _editorManager.RequestSceneRegionProps(region);
            _editorManager.SceneEditor.SetCurrentRegion(region);
        };
        Action onDelete = () =>
        {
            if (_editorManager.SceneEditor.IsEditingGivenRegion(region))
            {
                _editorManager.DestroyRightPane();
            }
            _editorManager.SceneEditor.DeleteRegion(region);
        };
        ControllerBase.AddEntityListItem(
            region,
            list,
            _regionLabelDict,
            onSelect,
            onDelete,
            labelTextSelector: r => r.Name
        );
    }

    public void AddPropToScenePropList(SceneProp prop)
    {
        Gui.Instance.TryFindById<ListView<GuiElement>>("scene-props-list", out var list);

        Action onSelect = () =>
        {
            _editorManager.RequestScenePropProps(prop);
            _editorManager.SceneEditor.SetCurrentProp(prop);
        };
        Action onDelete = () =>
        {
            if (_editorManager.SceneEditor.IsEditingGivenProp(prop))
            {
                _editorManager.DestroyRightPane();
            }
            _editorManager.SceneEditor.DeleteProp(prop);
        };
        ControllerBase.AddEntityListItem(
            prop,
            list,
            _propLabelDict,
            onSelect,
            onDelete,
            labelTextSelector: p => p.Name
        );
    }

    public void AddLightToSceneLightsList(SceneLight light)
    {
        Gui.Instance.TryFindById<ListView<GuiElement>>("scene-lights-list", out var list);

        Action onSelect = () =>
        {
            _editorManager.RequestSceneLightProps(light);
            _editorManager.SceneEditor.SetCurrentLight(light);
        };
        Action onDelete = () =>
        {
            if (_editorManager.SceneEditor.IsEditingGivenLight(light))
            {
                _editorManager.DestroyRightPane();
            }
            _editorManager.SceneEditor.DeleteLight(light);
        };
        ControllerBase.AddEntityListItem(
            light,
            list,
            _lightLabelDict,
            onSelect,
            onDelete,
            labelTextSelector: l => "Light"
        );
    }

    public void AddWalkbehindToSceneWalkbehindsList(SceneWalkbehind walkbehind)
    {
        Gui.Instance.TryFindById<ListView<GuiElement>>("scene-walkbehinds-list", out var list);

        Action onSelect = () =>
        {
            _editorManager.RequestSceneWalkbehindProps(walkbehind);
            _editorManager.SceneEditor.SetCurrentWalkbehind(walkbehind);
        };
        Action onDelete = () =>
        {
            if (_editorManager.SceneEditor.IsEditingGivenWalkbehind(walkbehind))
            {
                _editorManager.DestroyRightPane();
            }
            _editorManager.SceneEditor.DeleteWalkbehind(walkbehind);
        };
        ControllerBase.AddEntityListItem(
            walkbehind,
            list,
            _walkbehindLabelDict,
            onSelect,
            onDelete,
            labelTextSelector: w => w.Name
        );
    }

    private string UserRequestTextureFile()
    {
        var reset = new ManualResetEvent(false);
        string filename = null;
        Gtk.Application.Invoke(delegate
        {
            var fileChooserDialog = new Gtk.FileChooserNative(
                "Select scene texture.",
                null,
                Gtk.FileChooserAction.Open,
                "Open",
                "Cancel"
            );

            if (fileChooserDialog.Run() == (int)Gtk.ResponseType.Accept)
            {
                filename = fileChooserDialog.Filename;
            }
            fileChooserDialog.Destroy();
            reset.Set();
        });

        reset.WaitOne();

        return filename;
    }
    private string UserRequestActorFile()
    {
        var reset = new ManualResetEvent(false);
        string filename = null;
        Gtk.Application.Invoke(delegate
        {
            var fileChooserDialog = new Gtk.FileChooserNative(
                "Select an actor file.",
                null,
                Gtk.FileChooserAction.Open,
                "Open",
                "Cancel"
            );

            if (fileChooserDialog.Run() == (int)Gtk.ResponseType.Accept)
            {
                filename = fileChooserDialog.Filename;
            }
            fileChooserDialog.Destroy();
            reset.Set();
        });

        reset.WaitOne();

        return filename;
    }
}