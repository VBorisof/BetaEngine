using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Instances;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class ActorPutMethod : ICallable
{
    public int Arity() => 3;

    private readonly BDSMActor _caller;

    public ActorPutMethod(BDSMActor caller)
    {
        _caller = caller;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            interpreter.EventHandlers.OnActorPut(
                this,
                new ActorPutEventArgs(
                    context,
                    _caller,
                    (BDSMActor)arguments[0],
                    (int)(double)arguments[1],
                    (int)(double)arguments[2]
                )
            );
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(
                null,
                $"{_caller.DeclName}.put expects item and two integer args (x, y)."
            );
        }
        return null;
    }
}
