#nullable disable

using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Instances;

namespace BDSM.StandardLibrary;

public class PlayAnimationEventArgs : BDSMEventArgs
{
    public GameInstance What { get; }
    public string Name { get; }

    public PlayAnimationEventArgs(ExecutionContext executionContext, GameInstance what, string name) : base(executionContext)
    {
        What = what;
        Name = name;
    }
}