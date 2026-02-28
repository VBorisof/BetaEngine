using Beta.DI;
using Beta.Tutorials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Beta.Services;

public class TutorialService
{
    private readonly TutorialProvider _tutorialProvider;
    private Tutorial? _tutorial;

    private bool _isInit;
    public bool IsInTutorial { get; set; }

    public event EventHandler<RequestTutorialStepGuiEventArgs> RequestTutorialStepGui = (_, _) => { };
    public event EventHandler RequestTutorialBannerRemove = (_, _) => { };
    public event EventHandler TutorialEnded = (_, _) => { };
    public event EventHandler RequestGuiControl = (_, _) => { };
    public event EventHandler ReleaseGuiControl = (_, _) => { };

    public TutorialService()
    {
        _tutorialProvider = DependencyContainer.Instance.Get<TutorialProvider>();
        LoadTutorial();

        IsInTutorial = false;
    }

    public void DoIfAllowed(TutorialStepAction action, Action onAllowed)
    {
        if (!IsInTutorial)
        {
            onAllowed();
            return;
        }
        if (_tutorial is null)
        {
            throw new InvalidOperationException("Tutorial is not loaded.");
        }

        if (_tutorial.IsActionAllowed(action))
        {
            onAllowed();

            // See if this triggered the next step.
            _tutorial.OnTutorialAction(action);
        }
    }

    public void Update(GameTime gameTime)
    {
        if (IsInTutorial)
        {
            _tutorial!.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (IsInTutorial)
        {
            _tutorial!.Draw(spriteBatch);
        }
    }

    public void EndTutorial()
    {
        IsInTutorial = false;
        _isInit = false;
        TutorialEnded.Invoke(this, EventArgs.Empty);
    }

    public void BeginTutorialIfNotStarted()
    {
        if (_isInit)
        {
            return;
        }

        if (_tutorial is null)
        {
            throw new InvalidOperationException("Tutorial is not loaded.");
        }

        _tutorial.Init();
        _isInit = true;
        IsInTutorial = true;
    }

    public void GoToNextStep()
    {
        if (_tutorial is null)
        {
            throw new InvalidOperationException("Tutorial is not loaded.");
        }
        _tutorial.GoToNextStep();
    }

    public void GoToPrevStep()
    {
        if (_tutorial is null)
        {
            throw new InvalidOperationException("Tutorial is not loaded.");
        }
        _tutorial.GoToPrevStep();
    }

    public void LoadTutorial()
    {
        _tutorial = _tutorialProvider.GetTutorial();
        _tutorial.TutorialDone += (_, _) => EndTutorial();
        _tutorial.RequestTutorialStepGui += OnRequestTutorialStepGui;
        _tutorial.RequestRemoveTutorialBanner += OnRequestTutorialBannerRemove;
        _tutorial.RequestGuiControl += OnRequestGuiControl;
        _tutorial.ReleaseGuiControl += OnReleaseGuiControl;

        _tutorial.Init();
        IsInTutorial = true;
    }

    private void OnRequestTutorialBannerRemove(object? sender, EventArgs e)
    {
        RequestTutorialBannerRemove.Invoke(this, e);
    }

    private void OnRequestTutorialStepGui(object? sender, RequestTutorialStepGuiEventArgs e)
    {
        RequestTutorialStepGui.Invoke(sender, e);
    }

    private void OnReleaseGuiControl(object? sender, EventArgs e)
    {
        ReleaseGuiControl.Invoke(sender, e);
    }

    private void OnRequestGuiControl(object? sender, EventArgs e)
    {
        RequestGuiControl.Invoke(sender, e);
    }
}
