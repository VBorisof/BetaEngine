using BDSM.Instances;
using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class ItemPickupMethod : ICallable
{
    public int Arity() => 1;

    private BDSMActor _caller;

    public ItemPickupMethod(BDSMActor caller)
    {
        _caller = caller;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            interpreter.EventHandlers.OnItemPickup(this, new ItemPickupEventArgs(context, _caller, (BDSMActor)arguments[0]));
            return null;
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(null, $"{_caller.DeclName}.pickup() expects single Actor argument");
        }
    }
}