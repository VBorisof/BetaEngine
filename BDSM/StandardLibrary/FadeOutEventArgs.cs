using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class FadeOutEventArgs : BDSMEventArgs
{
    public double Speed { get; set; }

    public FadeOutEventArgs(ExecutionContext executionContext, double speed) : base(executionContext)
    {
        Speed = speed;
    }
}
