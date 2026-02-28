using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class StopSongEventArgs : BDSMEventArgs
{
    public StopSongEventArgs(ExecutionContext executionContext) : base(executionContext)
    {
    }
}