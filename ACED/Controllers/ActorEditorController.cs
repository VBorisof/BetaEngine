using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using aced.Models;
using Beta.DI;
using Beta.Entities.Animations;
using Beta.Gui;
using Beta.Gui.Elements;
using Beta.Gui.Events;
using Beta.Logging;

namespace aced.Controllers;

internal class ActorEditorController : GuiHandler<ActorEditorController>
{
    private readonly EditorManager _editorManager;

    // Change the label when we change the costume/animation name etc...
    private readonly Dictionary<Costume, Label> _costumeLabelDict = [];
    private readonly Dictionary<Animation, Label> _animationLabelDict = [];
    private readonly ILogger _logger;

    public ActorEditorController()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _editorManager = DependencyContainer.Instance.Get<EditorManager>();
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "actor-name-input")]
    public void HandleActorNameInput(string value)
    {
        _editorManager.ActorEditor.ChangeActorName(value);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "actor-speed-input")]
    public void HandleActorSpeedInput(string value)
    {
        var asFloat = float.Parse(value, CultureInfo.InvariantCulture);
        _editorManager.ActorEditor.ChangeActorSpeed(asFloat);
    }

    [HandlerFor(GuiEventType.LeftClick, "actor-add-costume-button")]
    public void HandleActorAddCostumeButton()
    {
        var costume = _editorManager.ActorEditor.AddCostume();

        AddCostumeToActorCostumeList(costume);

        _editorManager.ActorEditor.SetCurrentCostume(costume);
        _editorManager.RequestCostumeProps(costume);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "costume-name-input")]
    public void HandleCostumeNameInput(string value)
    {
        _editorManager.ActorEditor.ChangeCurrentCostumeName(value);
        _costumeLabelDict[_editorManager.ActorEditor.CurrentCostume].Text = value;
    }

    [HandlerFor(GuiEventType.LeftClick, "costume-add-animation-button")]
    public void HandleCostumeAddAnimationButton()
    {
        var selectedFilePaths = UserRequestAnimationFiles();
        if (selectedFilePaths.Count() == 0)
        {
            _logger.Info("No animation files selected.");
            return;
        }

        var anim = _editorManager.ActorEditor
            .AddAnimationToCurrentCostume(selectedFilePaths);

        _editorManager.ActorEditor.SetCurrentAnimation(anim);

        AddAnimationToCostumeAnimationList(anim);

        _editorManager.RequestAnimationProps(anim);
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "animation-name-input")]
    public void HandleAnimationNameInput(string value)
    {
        _editorManager.ActorEditor.ChangeCurrentAnimationName(value);
        _animationLabelDict[_editorManager.ActorEditor.CurrentAnimation].Text = value;
    }

    [HandlerFor(GuiEventType.TextInputValueChanged, "animation-speed-input")]
    public void HandleAnimationSpeedInput(string value)
    {
        var speed = float.Parse(value, CultureInfo.InvariantCulture);
        _editorManager.ActorEditor.ChangeCurrentAnimationSpeed(speed);
    }

    [HandlerFor(GuiEventType.CheckboxToggle, "animation-repeat-checkbox")]
    public void HandleAnimationRepeatInput(bool value)
    {
        _editorManager.ActorEditor.ChangeCurrentAnimationRepeat(value);
    }

    public void AddCostumeToActorCostumeList(Costume costume)
    {
        Gui.Instance.TryFindById<ListView<GuiElement>>("actor-costumes-list", out var list);
        Action onSelect = () =>
        {
            _editorManager.ActorEditor.SetCurrentCostume(costume);
            _editorManager.RequestCostumeProps(costume);
        };
        Action onDelete = () =>
        {
            if (costume == _editorManager.ActorEditor.CurrentCostume)
            {
                _editorManager.DestroyRightPane();
            }
            _editorManager.ActorEditor.DeleteCostume(costume);
        };
        ControllerBase.AddEntityListItem(
            costume,
            list,
            _costumeLabelDict,
            onSelect,
            onDelete,
            (costume) => costume.Name
        );
    }

    public void AddAnimationToCostumeAnimationList(Animation anim)
    {
        Gui.Instance.TryFindById<ListView<GuiElement>>("costume-animations-list", out var list);

        Action onSelect = () =>
        {
            _editorManager.ActorEditor.SetCurrentAnimation(anim);
            _editorManager.RequestAnimationProps(anim);
        };
        Action onDelete = () =>
        {
            if (anim == _editorManager.ActorEditor.CurrentAnimation)
            {
                _editorManager.DestroyRightBottomPane();
            }
            _editorManager.ActorEditor.DeleteAnimation(anim);
        };
        ControllerBase.AddEntityListItem(
            anim,
            list,
            _animationLabelDict,
            onSelect,
            onDelete,
            (anim) => anim.Name
        );
    }

    private string[] UserRequestAnimationFiles()
    {
        var reset = new ManualResetEvent(false);
        string[] selectedFilePaths = { };
        Gtk.Application.Invoke(delegate
        {
            var fileChooserDialog = new Gtk.FileChooserNative(
                "Select animation files.",
                null,
                Gtk.FileChooserAction.Open,
                "Open",
                "Cancel"
            )
            {
                SelectMultiple = true
            };

            if (fileChooserDialog.Run() == (int)Gtk.ResponseType.Accept)
            {
                selectedFilePaths = fileChooserDialog.Filenames;
            }

            fileChooserDialog.Destroy();
            reset.Set();
        });

        reset.WaitOne();
        return selectedFilePaths;
    }
}