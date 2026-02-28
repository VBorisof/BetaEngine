using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlayVideoEventArgs : BDSMEventArgs
{
    public string Name { get; }

    public PlayVideoEventArgs(ExecutionContext executionContext, string name) : base(executionContext)
    {
        Name = name;
    }
}
