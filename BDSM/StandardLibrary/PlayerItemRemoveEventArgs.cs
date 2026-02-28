using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Instances;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlayerItemRemoveEventArgs : BDSMEventArgs
{
    public BDSMActor Who { get; }
    public BDSMActor What { get; }

    public PlayerItemRemoveEventArgs(ExecutionContext context, BDSMActor who, BDSMActor what) : base(context)
    {
        Who = who;
        What = what;
    }
}