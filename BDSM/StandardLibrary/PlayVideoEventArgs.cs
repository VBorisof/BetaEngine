using BDSM.Events;
using BDSM.ExecutionContexts;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlayVideoArgs : BDSMEventArgs
{
    public string Name { get; }

    public PlayVideoArgs(ExecutionContext executionContext, string name) : base(executionContext)
    {
        Name = name;
    }
}
