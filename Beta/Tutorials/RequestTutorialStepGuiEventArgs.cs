using System;

namespace Beta.Tutorials;

public class RequestTutorialStepGuiEventArgs : EventArgs
{
    public required TutorialStepStyle Style { get; init; }
    public required string Name { get; init; }
}
