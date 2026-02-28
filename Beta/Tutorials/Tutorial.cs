using Beta.CommandManagement;
using Beta.DI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Beta.Tutorials;

public class Tutorial
{
    [JsonPropertyName("steps")]
    public TutorialStep[] Steps { get; set; } = [];

    private readonly CommandManager _commandManager;
    private int _currentStepIdx;

    private bool _actionMatched;

    public event EventHandler<RequestTutorialStepGuiEventArgs> RequestTutorialStepGui = (_, _) => { };
    public event EventHandler RequestRemoveTutorialBanner = (_, _) => { };
    public event EventHandler RequestGuiControl = (_, _) => { };
    public event EventHandler ReleaseGuiControl = (_, _) => { };
    public event EventHandler TutorialDone = (_, _) => { };

    public Tutorial(TutorialStep[] steps)
    {
        Steps = steps;
        _commandManager = DependencyContainer.Instance.Get<CommandManager>();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        GetCurrentStep().Draw(spriteBatch);
    }

    private int _msElapsedAfterMatch;
    public void Update(GameTime gameTime)
    {
        if (_actionMatched)
        {
            var timeOk = GetCurrentStep().WaitMsOnMatch is null || GetCurrentStep().WaitMsOnMatch < _msElapsedAfterMatch;

            if (timeOk && !_commandManager.IsBusy())
            {
                OnFinishMatchedAction();
            }

            _msElapsedAfterMatch += gameTime.ElapsedGameTime.Milliseconds;
        }
    }

    public TutorialStep GetCurrentStep()
    {
        return Steps[_currentStepIdx];
    }

    public void Init()
    {
        _currentStepIdx = 0;
        InitStep(Steps[_currentStepIdx]);
    }

    public void GoToNextStep()
    {
        if (_currentStepIdx < Steps.Length - 1)
        {
            ++_currentStepIdx;
            InitStep(Steps[_currentStepIdx]);
        }
        else
        {
            OnTutorialDone();
        }
    }
    public void GoToPrevStep()
    {
        if (_currentStepIdx > 0)
        {
            --_currentStepIdx;
            InitStep(Steps[_currentStepIdx]);
        }
    }

    private void InitStep(TutorialStep tutorialStep)
    {
        RequestTutorialStepGui.Invoke(this, new RequestTutorialStepGuiEventArgs
        {
            Style = tutorialStep.StepStyle,
            Name = tutorialStep.Name
        });

        if (tutorialStep.StepStyle == TutorialStepStyle.Screen)
        {
            RequestGuiControl.Invoke(this, EventArgs.Empty);
        }
        if (tutorialStep.StepStyle == TutorialStepStyle.Banner)
        {
            ReleaseGuiControl.Invoke(this, EventArgs.Empty);
        }
    }

    public void OnTutorialAction(TutorialStepAction action)
    {
        var currentStep = GetCurrentStep();
        if (IsMatch(currentStep, action))
        {
            _actionMatched = true;
            _msElapsedAfterMatch = 0;
            RemoveTutorialBannerIfPresent(currentStep);
        }
    }

    private void RemoveTutorialBannerIfPresent(TutorialStep step)
    {
        if (step.StepStyle == TutorialStepStyle.Banner)
        {
            RequestRemoveTutorialBanner.Invoke(this, EventArgs.Empty);
        }
    }

    public void OnFinishMatchedAction()
    {
        _actionMatched = false;

        ++_currentStepIdx;
        if (_currentStepIdx == Steps.Length)
        {
            OnTutorialDone();
        }
        else
        {
            InitStep(Steps[_currentStepIdx]);
        }
    }

    private void OnTutorialDone()
    {
        if (!Settings.IsTutorialDebug)
        {
            ReleaseGuiControl.Invoke(this, EventArgs.Empty);
            TutorialDone.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsActionAllowed(TutorialStepAction action)
    {
        // Don't do anything if we've completed the step.
        if (_actionMatched)
        {
            return false;
        }

        var currentStep = GetCurrentStep();

        if (currentStep.AllowedActions.Any(
            a => a.ActionType == TutorialStepActionType.All
        ))
        {
            return true;
        }

        return action.IsAllowed(currentStep.AllowedActions);
    }

    private bool IsMatch(TutorialStep currentStep, TutorialStepAction action)
    {
        if (currentStep.ActionToMatch is null)
        {
            return true;
        }

        return currentStep.ActionToMatch.IsMatch(action);
    }
}