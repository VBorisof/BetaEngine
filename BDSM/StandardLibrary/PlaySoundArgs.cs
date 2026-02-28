using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlaySoundEventArgs : BDSMEventArgs
{
    public string Name { get; private set; }

    public PlaySoundEventArgs(ExecutionContext executionContext, string name) : base(executionContext)
    {
        Name = name;
    }
}
