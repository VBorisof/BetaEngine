using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Instances;
using BDSM.Runtime;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class ActorInterruptMethod : ICallable
{
    public int Arity() => 0;
    private readonly BDSMActor _caller;

    public ActorInterruptMethod(BDSMActor caller)
    {
        _caller = caller;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        var contextOverride = ExecutionContext.Actor(_caller.DeclName);
        interpreter.EventHandlers.OnActorInterrupt(this, new ActorInterruptEventArgs(contextOverride));
        return null;
    }
}
