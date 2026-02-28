#nullable disable

using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Instances;

namespace BDSM.StandardLibrary;

public class ActorSayEventArgs : BDSMEventArgs
{
    public BDSMActor Who { get; set; }
    public string What { get; set; }

    public ActorSayEventArgs(ExecutionContext context, BDSMActor who, string what) : base(context)
    {
        Who = who;
        What = what;
    }
}
