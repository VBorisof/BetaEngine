using System.Diagnostics;
using System.Threading;
using Beta.DI;
using Beta.Gui;
using Beta.Gui.Elements;
using Beta.Gui.Events;
using Beta.Logging;

namespace aced.Controllers;

internal class SceneDropdownController : GuiHandler<SceneDropdownController>
{
    private readonly DropdownMenu _sceneDropdown;
    private readonly EditorManager _editorManager;
    private readonly ILogger _logger;

    public SceneDropdownController()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _editorManager = DependencyContainer.Instance.Get<EditorManager>();
        Gui.Instance.TryFindById("scene-dropdown", out _sceneDropdown);
    }

    [HandlerFor(GuiEventType.LeftClick, "new-scene-button")]
    public void HandleNewSceneButton()
    {
        _sceneDropdown.Close();

        var messageBox = Gui.Instance.OkCancelMessageBox("New scene, are you sure?");
        messageBox.OnOk += (_, __) =>
        {
            _editorManager.SceneEditor.ResetEnvironment();
            _editorManager.RequestSceneEditor();
        };
    }

    [HandlerFor(GuiEventType.LeftClick, "open-scene-button")]
    public void HandleOpenSceneButton()
    {
        _sceneDropdown.Close();

        var reset = new ManualResetEvent(false);
        string filename = null;
        Gtk.Application.Invoke(delegate
        {
            var fileChooserDialog = new Gtk.FileChooserNative(
                "Select scene.",
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
            _editorManager.ActorEditor.DestroyEnvironment();
            _editorManager.SceneEditor.DestroyEnvironment();
            _editorManager.SceneEditor.OpenScene(filename);

            // Hacky, cause we have some side effects during load:
            _editorManager.SceneEditor.ResetState();

            _editorManager.RequestSceneEditor();
        }
    }

    [HandlerFor(GuiEventType.LeftClick, "export-scene-button")]
    public void HandleExportSceneButton()
    {
        _sceneDropdown.Close();
        _editorManager.SceneEditor.Export();
        _logger.Info();
        _logger.Info("  Scene Export finished.");
        _logger.Info();
    }

    [HandlerFor(GuiEventType.LeftClick, "export-scene-build-button")]
    public void HandleExportSceneAndBuildButton()
    {
        _sceneDropdown.Close();
        _editorManager.SceneEditor.Export();
        _logger.Info();
        _logger.Info("  Scene Export finished.");
        _logger.Info();
        BuildGame();
    }

    private void BuildGame()
    {
        _logger.Info("==============================================");
        _logger.Info("============ GAME BUILD TRIGGERED ============");
        _logger.Info();
        var start = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build",
            WorkingDirectory = Settings.GAME_PWD
        };
        using (var proc = Process.Start(start))
        {
            proc.WaitForExit();
        }
        _logger.Info();
        _logger.Info("============ GAME BUILD COMPLETE =============");
        _logger.Info("==============================================");

        var currentSceneName = _editorManager.SceneEditor.CurrentScene.Name;
        BuildTools.WriteStartupScript(currentSceneName);
    }
}