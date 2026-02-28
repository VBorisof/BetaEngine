using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class WaitFunction : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            int timeout = (int)((double)arguments[0]);
            interpreter.EventHandlers.OnWait(this, new WaitEventArgs(context, timeout));
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(null, "wait() expects a single number argument.");
        }
        return null;
    }
}
