#nullable disable

using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Instances;

namespace BDSM.StandardLibrary;

public class ActorPutEventArgs : BDSMEventArgs
{
    public BDSMActor Who { get; set; }
    public BDSMActor What { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public ActorPutEventArgs(ExecutionContext context, BDSMActor who, BDSMActor what, int x, int y) : base(context)
    {
        Who = who;
        What = what;
        X = x;
        Y = y;
    }
}