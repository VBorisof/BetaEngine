#nullable disable

using BDSM.ExecutionContexts;

namespace BDSM.Events;

public class BDSMEventArgs
{
    public ExecutionContext ExecutionContext { get; set; }

    public BDSMEventArgs(ExecutionContext executionContext)
    {
        ExecutionContext = executionContext;
    }
}