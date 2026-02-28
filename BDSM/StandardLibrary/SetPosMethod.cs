using BDSM.Instances;
using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class SetPosMethod : ICallable
{
    public int Arity() => 2;

    private GameInstance _caller;

    public SetPosMethod(GameInstance caller)
    {
        _caller = caller;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            interpreter.EventHandlers.OnSetPos(
                this,
                new SetPosEventArgs(context, _caller,(int)(double)arguments[0], (int)(double) arguments[1])
            );
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(
                null,
                $"{_caller.DeclName}.setpos expects two integer args (x, y)."
            );
        }
        return null;
    }
}

