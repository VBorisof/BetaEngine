using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlaySongEventArgs : BDSMEventArgs
{
    public string Name { get; private set; }

    public PlaySongEventArgs(ExecutionContext executionContext, string name) : base(executionContext)
    {
        Name = name;
    }
}
