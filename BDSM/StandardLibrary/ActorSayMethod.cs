using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Instances;
using BDSM.Runtime;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class ActorSayMethod : ICallable
{
    public int Arity() => 1;

    private readonly BDSMActor _caller;

    public ActorSayMethod(BDSMActor caller)
    {
        _caller = caller;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        interpreter.EventHandlers.OnActorSay(this, new ActorSayEventArgs(context, _caller, (string)arguments[0]));
        return null;
    }
}