using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class EndGameEventArgs : BDSMEventArgs
{
    public EndGameEventArgs(ExecutionContext executionContext) : base(executionContext)
    {
    }
}
