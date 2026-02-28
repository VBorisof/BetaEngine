using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class CinematicStartEventArgs : BDSMEventArgs
{
    public CinematicStartEventArgs(ExecutionContext executionContext) : base(executionContext)
    {
    }
}
