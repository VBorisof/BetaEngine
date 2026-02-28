using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class CinematicEndEventArgs : BDSMEventArgs
{
    public CinematicEndEventArgs(ExecutionContext executionContext) : base(executionContext)
    {
    }
}
