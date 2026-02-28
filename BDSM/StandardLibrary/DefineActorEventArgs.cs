using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Instances;

#nullable disable

namespace BDSM.StandardLibrary;

public class DefineActorEventArgs : BDSMEventArgs
{
    public BDSMActor Who { get; set; }

    public DefineActorEventArgs(ExecutionContext context, BDSMActor who) : base(context)
    {
        Who = who;
    }
}
