using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class RequestMainMenuFunction : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            interpreter.EventHandlers.OnRequestMainMenu(this, new RequestMainMenuEventArgs(context, (bool) arguments[0]));
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(
                null,
                "requestmainmenu expects bool (isStarted) as the only argument."
            );
        }

        return null;
    }
}