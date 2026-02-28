using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class RequestTutorialEventArgs : BDSMEventArgs
{
    public RequestTutorialEventArgs(ExecutionContext executionContext) : base(executionContext)
    {
    }
}
