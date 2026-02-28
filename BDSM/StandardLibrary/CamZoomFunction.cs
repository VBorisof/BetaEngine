using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class CamZoomFunction : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            interpreter.EventHandlers.OnCamZoom(this, new CamZoomEventArgs(context, (int)(double)arguments[0]));
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(
                null,
                "zoomcam expects integer as the only argument."
            );
        }

        return null;
    }
}
