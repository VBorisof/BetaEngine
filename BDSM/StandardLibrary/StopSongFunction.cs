using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class StopSongFunction : ICallable
{
    public int Arity() => 0;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        interpreter.EventHandlers.OnStopSong(this, new StopSongEventArgs(context));
        return null;
    }
}
