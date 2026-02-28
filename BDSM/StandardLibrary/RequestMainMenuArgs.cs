using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class RequestMainMenuEventArgs : BDSMEventArgs
{
    public bool IsStarted { get; }

    public RequestMainMenuEventArgs(ExecutionContext executionContext, bool isStarted) : base(executionContext)
    {
        IsStarted = isStarted;
    }
}