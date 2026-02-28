using System;
using System.Diagnostics;
using Beta.DI;
using Beta.Gui.Events;
using Beta.Logging;

namespace aced.Controllers;

internal class TopbarController : GuiHandler<TopbarController>
{
    public EventHandler OnQuit = (_, _) => { };
    private readonly EditorManager _editorManager;
    private readonly ILogger _logger;

    public TopbarController()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _editorManager = DependencyContainer.Instance.Get<EditorManager>();
    }

    [HandlerFor(GuiEventType.LeftClick, "draw-exits-button")]
    public void HandleDrawExitsButton()
    {
        Settings.IsDrawExits = !Settings.IsDrawExits;
    }

    [HandlerFor(GuiEventType.LeftClick, "draw-actors-button")]
    public void HandleDrawActorsButton()
    {
        Settings.IsDrawActors = !Settings.IsDrawActors;
    }

    [HandlerFor(GuiEventType.LeftClick, "draw-scale-map-button")]
    public void HandleDrawScaleMapButton()
    {
        Settings.IsDrawScaleMap = !Settings.IsDrawScaleMap;
    }

    [HandlerFor(GuiEventType.LeftClick, "draw-walkables-button")]
    public void HandleDrawWalkablesButton()
    {
        Settings.IsDrawWalkableAreas = !Settings.IsDrawWalkableAreas;
    }

    [HandlerFor(GuiEventType.CheckboxToggle, "dryrun-checkbox")]
    public void HandleDryRunChanged(bool value)
    {
        Settings.IsDryRun = value;
    }

    [HandlerFor(GuiEventType.LeftClick, "run-button")]
    public void HandleRunButton()
    {
        _logger.Debug();
        RunGame();
    }

    [HandlerFor(GuiEventType.LeftClick, "quit-button")]
    public void Exit()
    {
        _editorManager.RequestExit();
    }

    public void RunGame()
    {
        if (_editorManager.EditorState != EditorState.SceneEdit)
        {
            _logger.Info("Not running the game: Not in Scene Editor.");
            return;
        }

        _editorManager.SceneEditor.ResetState();

        var currentSceneName = _editorManager.SceneEditor.CurrentScene.Name;
        BuildTools.WriteStartupScript(currentSceneName);

        var start = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run {Settings.LAUNCH_SCRIPT_PATH}",
            WorkingDirectory = Settings.GAME_PWD
        };
        using var proc = Process.Start(start);

        proc.WaitForExit();
    }
}