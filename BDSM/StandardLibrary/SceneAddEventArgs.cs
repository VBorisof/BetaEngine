using BDSM.Events;
using BDSM.ExecutionContexts;

using BDSM.Instances;
#nullable disable

namespace BDSM.StandardLibrary;

public class SceneAddArgs : BDSMEventArgs
{
    public BDSMScene Scene { get; set; }
    public GameInstance What { get; set; }

    public SceneAddArgs(ExecutionContext context, BDSMScene scene, GameInstance what) : base(context)
    {
        Scene = scene;
        What = what;
    }
}