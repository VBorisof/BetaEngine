using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Instances;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlayerItemAddEventArgs : BDSMEventArgs
{
    public BDSMActor Who { get; }
    public BDSMActor What { get; }

    public PlayerItemAddEventArgs(ExecutionContext context, BDSMActor who, BDSMActor what) : base(context)
    {
        Who = who;
        What = what;
    }
}