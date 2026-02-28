using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Instances;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlayAnimationMethod : ICallable
{
    private readonly GameInstance _caller;

    public int Arity() => 1;

    public PlayAnimationMethod(GameInstance caller)
    {
        _caller = caller;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            interpreter.EventHandlers.OnPlayAnimation(this, new PlayAnimationEventArgs(context, _caller, (string) arguments[0]));
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(
                null,
                $"{_caller.DeclName}.playanimation expects an animation name."
            );
        }
        return null;
    }
}