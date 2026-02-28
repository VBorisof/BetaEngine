using BDSM.Runtime;
using BDSM.Functions;
using BDSM.ExecutionContexts;

using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class CinematicStartFunction : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            var func = (ICallable)arguments[0];
            if (func.Arity() > 0)
            {
                throw new RuntimeError(null, "playcinematic function must not take args.");
            }
            interpreter.EventHandlers.OnCinematicStart(this, new CinematicStartEventArgs(context));
            func.Call(interpreter, null, context);
            interpreter.EventHandlers.OnCinematicEnd(this, new CinematicEndEventArgs(context));
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(null, "playcinematic expects a function parameter.");
        }

        return null;
    }
}


/*
public class CinematicEndFunction : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            var func = (ICallable)arguments[0];
            if (func.Arity() > 0)
            {
                throw new RuntimeError(null, "playcinematic function must not take args.");
            }
            interpreter.OnCinematicStart(this, null);
            func.Call(interpreter, null, context);
            interpreter.OnCinematicEnd(this, null);
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(null, "playcinematic expects a function parameter.");
        }

        return null;
    }
}
*/