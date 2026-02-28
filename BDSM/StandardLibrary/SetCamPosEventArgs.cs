using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class SetCamPosEventArgs : BDSMEventArgs
{
    public int X { get; set; }
    public int Y { get; set; }

    public SetCamPosEventArgs(ExecutionContext executionContext, int x, int y) : base(executionContext)
    {
        X = x;
        Y = y;
    }
}

