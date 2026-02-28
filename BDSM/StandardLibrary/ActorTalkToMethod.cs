using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Instances;
using BDSM.Runtime;
using System.Collections.Generic;

namespace BDSM.StandardLibrary;

#nullable disable

public class ActorTalkToMethod : ICallable
{
    public int Arity() => 3;

    private readonly BDSMActor _caller;

    public ActorTalkToMethod(BDSMActor caller)
    {
        _caller = caller;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        interpreter.EventHandlers.OnActorTalkTo(
            this,
            new ActorTalkToEventArgs(
                context,
                _caller,
                (BDSMActor)arguments[0],
                (int)(double)arguments[1],
                (bool)arguments[2]
            )
        );
        return null;
    }
}