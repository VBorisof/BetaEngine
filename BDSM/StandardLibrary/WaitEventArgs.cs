using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class WaitEventArgs : BDSMEventArgs
{
    public int TimeoutMillis { get; }

    public WaitEventArgs(ExecutionContext executionContext, int timeoutMillis) : base(executionContext)
    {
        TimeoutMillis = timeoutMillis;
    }
}
