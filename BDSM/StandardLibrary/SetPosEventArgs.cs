using BDSM.Events;
using BDSM.ExecutionContexts;

using BDSM.Instances;
#nullable disable

namespace BDSM.StandardLibrary;

public class SetPosEventArgs : BDSMEventArgs
{
    public GameInstance What { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public SetPosEventArgs(ExecutionContext context, GameInstance what, int x, int y) : base(context)
    {
        What = what;
        X = x;
        Y = y;
    }
}
