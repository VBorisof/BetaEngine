using BDSM.Instances;
using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System.Collections.Generic;

namespace BDSM.StandardLibrary;

#nullable disable

public class SetSceneFunction : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        interpreter.EventHandlers.OnSetScene(this, new SetSceneEventArgs(context, (BDSMScene)arguments[0]));
        return null;
    }
}
