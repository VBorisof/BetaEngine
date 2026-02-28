using BDSM.Events;
using BDSM.ExecutionContexts;
using BDSM.Instances;

#nullable disable

namespace BDSM.StandardLibrary;

public class DefineSceneEventArgs : BDSMEventArgs
{
    public BDSMScene Scene { get; set; }

    public DefineSceneEventArgs(ExecutionContext context, BDSMScene scene) : base(context)
    {
        Scene = scene;
    }
}
