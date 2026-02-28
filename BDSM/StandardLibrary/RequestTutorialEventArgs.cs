using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class RequestTutorialArgs : BDSMEventArgs
{
    public RequestTutorialArgs(ExecutionContext executionContext) : base(executionContext)
    {
    }
}
