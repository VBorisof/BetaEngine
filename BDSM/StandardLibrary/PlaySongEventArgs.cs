using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlaySongArgs : BDSMEventArgs
{
    public string Name { get; private set; }

    public PlaySongArgs(ExecutionContext executionContext, string name) : base(executionContext)
    {
        Name = name;
    }
}
