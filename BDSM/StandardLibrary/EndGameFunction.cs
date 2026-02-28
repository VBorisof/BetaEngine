using BDSM.Runtime;
using BDSM.Functions;
using BDSM.ExecutionContexts;

#nullable disable

using System.Collections.Generic;

namespace BDSM.StandardLibrary;

public class EndGameFunction : ICallable
{
    public int Arity() => 0;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        interpreter.EventHandlers.OnEndGame(this, new EndGameEventArgs(context));
        return null;
    }
}
