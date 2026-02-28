#nullable disable

using BDSM.Events;
using BDSM.ExecutionContexts;

namespace BDSM.StandardLibrary;

public class CamZoomEventArgs : BDSMEventArgs
{
    public int Zoom { get; set; }

    public CamZoomEventArgs(ExecutionContext executionContext, int zoom) : base(executionContext)
    {
        Zoom = zoom;
    }
}
