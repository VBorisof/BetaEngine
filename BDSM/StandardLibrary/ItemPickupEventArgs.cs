using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Instances;

#nullable disable

namespace BDSM.StandardLibrary;

public class ItemPickupEventArgs : BDSMEventArgs
{
    public BDSMActor Who { get; set; }
    public BDSMActor What { get; set; }

    public ItemPickupEventArgs(ExecutionContext context, BDSMActor who, BDSMActor what) : base(context)
    {
        Who = who;
        What = what;
    }
}
