using System.Threading;
using Beta.DI;
using Beta.Gui;
using Beta.Gui.Elements;
using Beta.Gui.Events;
using Beta.Logging;

namespace aced.Controllers;

internal class ActorDropdownController : GuiHandler<ActorDropdownController>
{
    private readonly DropdownMenu _actorDropdown;
    private readonly EditorManager _editorManager;
    private readonly ILogger _logger;

    public ActorDropdownController()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _editorManager = DependencyContainer.Instance.Get<EditorManager>();
        Gui.Instance.TryFindById("actor-dropdown", out _actorDropdown);
        Gui.Instance.TryFindById<Checkbox>("dryrun-checkbox", out var dryrunCheckbox);

        dryrunCheckbox.Value = Settings.IsDryRun;
    }

    [HandlerFor(GuiEventType.LeftClick, "new-actor-button")]
    public void HandleNewActorButton()
    {
        _actorDropdown.Close();

        var messageBox = Gui.Instance.OkCancelMessageBox("New actor, are you sure?");
        messageBox.OnOk += (_, __) =>
        {
            _editorManager.ActorEditor.ResetEnvironment();
            _editorManager.RequestActorEditor();
        };
    }

    [HandlerFor(GuiEventType.LeftClick, "open-actor-button")]
    public void HandleOpenActorButton()
    {
        _actorDropdown.Close();

        var reset = new ManualResetEvent(false);
        string filename = null;
        Gtk.Application.Invoke(delegate
        {
            var fileChooserDialog = new Gtk.FileChooserNative(
                "Select actor.",
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

        if (!string.IsNullOrWhiteSpace(filename))
        {
            var actorData = _editorManager.ActorEditor.ReadActor(filename);
            _editorManager.RequestActorEditor();
        }
    }

    [HandlerFor(GuiEventType.LeftClick, "export-actor-button")]
    public void HandleExportActorButton()
    {
        _actorDropdown.Close();
        _editorManager.ActorEditor.ExportActor();
        _logger.Info();
        _logger.Info("  Actor Export finished.");
        _logger.Info();
    }
}