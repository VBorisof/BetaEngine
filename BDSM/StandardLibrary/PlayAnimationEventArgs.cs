using BDSM.Instances;
using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlayAnimationArgs : BDSMEventArgs
{
    public GameInstance What { get; }
    public string Name { get; }

    public PlayAnimationArgs(ExecutionContext executionContext, GameInstance what, string name) : base(executionContext)
    {
        What = what;
        Name = name;
    }
}