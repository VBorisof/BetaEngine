#nullable disable

using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Instances;

namespace BDSM.StandardLibrary;

public class ActorMoveEventArgs : BDSMEventArgs
{
    public BDSMActor Who { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public ActorMoveEventArgs(ExecutionContext context, BDSMActor who, int x, int y) : base(context)
    {
        Who = who;
        X = x;
        Y = y;
    }
}