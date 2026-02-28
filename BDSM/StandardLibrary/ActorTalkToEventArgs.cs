using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Instances;

namespace BDSM.StandardLibrary;
#nullable disable

public class ActorTalkToEventArgs : BDSMEventArgs
{
    public BDSMActor Who { get; set; }
    public BDSMActor To { get; set; }
    public int NodeIndex { get; set; }
    public bool IsWalkTo { get; set; }

    public ActorTalkToEventArgs(ExecutionContext context, BDSMActor who, BDSMActor to, int nodeIndex, bool walkTo) : base(context)
    {
        Who = who;
        To = to;
        NodeIndex = nodeIndex;
        IsWalkTo = walkTo;
    }
}

