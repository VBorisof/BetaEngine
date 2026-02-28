using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlaySoundArgs : BDSMEventArgs
{
    public string Name { get; private set; }

    public PlaySoundArgs(ExecutionContext executionContext, string name) : base(executionContext)
    {
        Name = name;
    }
}
