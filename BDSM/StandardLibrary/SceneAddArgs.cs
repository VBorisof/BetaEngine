using BDSM.Events;
using BDSM.ExecutionContexts;

using BDSM.Instances;
#nullable disable

namespace BDSM.StandardLibrary;

public class SceneAddEventArgs : BDSMEventArgs
{
    public BDSMScene Scene { get; set; }
    public GameInstance What { get; set; }

    public SceneAddEventArgs(ExecutionContext context, BDSMScene scene, GameInstance what) : base(context)
    {
        Scene = scene;
        What = what;
    }
}