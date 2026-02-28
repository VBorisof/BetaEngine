using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class RequestMainMenuArgs : BDSMEventArgs
{
    public bool IsStarted { get; }

    public RequestMainMenuArgs(ExecutionContext executionContext, bool isStarted) : base(executionContext)
    {
        IsStarted = isStarted;
    }
}