#nullable disable

using BDSM.Events;
using BDSM.ExecutionContexts;

namespace BDSM.StandardLibrary;

public class ActorInterruptEventArgs : BDSMEventArgs
{
    public ActorInterruptEventArgs(ExecutionContext executionContext) : base(executionContext)
    {
    }
}

