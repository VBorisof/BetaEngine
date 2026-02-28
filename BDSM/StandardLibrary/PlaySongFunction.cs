using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class PlaySongFunction : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try
        {
            interpreter.EventHandlers.OnPlaySong(this, new PlaySongEventArgs(context, (string)arguments[0]));
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(
                null,
                "playsong expects song name as the only argument."
            );
        }

        return null;
    }
}
