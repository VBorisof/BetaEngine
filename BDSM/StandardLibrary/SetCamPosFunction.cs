using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class SetCamPosFunction : ICallable
{
    public int Arity() => 2;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            interpreter.EventHandlers.OnSetCamPos(this, new SetCamPosEventArgs(context, (int)((double)arguments[0]), (int)((double) arguments[1])));
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(
                null,
                "setcampos expects (x, y) integers as the only argument."
            );
        }

        return null;
    }
}

