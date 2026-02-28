using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class FadeInEventArgs : BDSMEventArgs
{
    public double Speed { get; set; }

    public FadeInEventArgs(ExecutionContext executionContext, double speed) : base(executionContext)
    {
        Speed = speed;
    }
}

