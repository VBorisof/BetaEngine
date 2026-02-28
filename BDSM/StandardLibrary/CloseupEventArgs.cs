#nullable disable

using BDSM.Events;
using BDSM.ExecutionContexts;

namespace BDSM.StandardLibrary;

public class CloseupEventArgs : BDSMEventArgs
{
    public string Name { get; set; }

    public CloseupEventArgs(ExecutionContext executionContext, string name) : base(executionContext)
    {
        Name = name;
    }
}
