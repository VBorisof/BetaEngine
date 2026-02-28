using BDSM.Instances;
using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlayerItemRemoveMethod : ICallable
{
    public int Arity() => 1;

    private BDSMActor _caller;

    public PlayerItemRemoveMethod(BDSMActor caller)
    {
        _caller = caller;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            interpreter.EventHandlers.OnPlayerItemRemove(this, new PlayerItemRemoveEventArgs(context, _caller, (BDSMActor)arguments[0]));
            return null;
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(null, $"{_caller.DeclName}.removeitem() expects single Actor argument.");
        }
    }
}