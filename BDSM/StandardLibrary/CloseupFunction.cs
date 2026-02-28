using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class CloseupFunction : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        interpreter.EventHandlers.OnCloseup(this, new CloseupEventArgs(context, (string)arguments[0]));
        return null;
    }
}
