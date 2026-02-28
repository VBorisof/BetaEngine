using BDSM.Events;
using BDSM.Instances;
using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class SetPlayerEventArgs : BDSMEventArgs
{
    public BDSMActor Who { get; set; }

    public SetPlayerEventArgs(ExecutionContext context, BDSMActor who) : base(context)
    {
        Who = who;
    }
}

public class SetPlayerFunction : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        interpreter.EventHandlers.OnSetPlayer(this, new SetPlayerEventArgs(context, (BDSMActor)arguments[0]));
        return null;
    }
}