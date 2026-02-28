using BDSM.ExecutionContexts;

using BDSM.Functions;
using BDSM.Instances;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class ActorMoveMethod : ICallable
{
    public int Arity() => 2;

    private readonly BDSMActor _caller;

    public ActorMoveMethod(BDSMActor caller)
    {
        _caller = caller;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            interpreter.EventHandlers.OnActorMove(
                this,
                new ActorMoveEventArgs(context, _caller, (int)(double)arguments[0], (int)(double) arguments[1])
            );
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(
                null,
                $"{_caller.DeclName}.move expects two integer args (x, y)."
            );
        }
        return null;
    }
}
