using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Instances;

namespace BDSM.StandardLibrary;

public class SetSceneEventArgs : BDSMEventArgs
{
    public BDSMScene Scene { get; }

    public SetSceneEventArgs(ExecutionContext executionContext, BDSMScene scene) : base(executionContext)
    {
        Scene = scene;
    }
}