using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlayVideoFunction : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            interpreter.EventHandlers.OnPlayVideo(this, new PlayVideoEventArgs(context, (string)arguments[0]));
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(
                null,
                "playvideo expects video name as the only argument."
            );
        }

        return null;
    }
}
