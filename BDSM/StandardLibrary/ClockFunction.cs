using BDSM.Runtime;
using BDSM.Functions;
using BDSM.ExecutionContexts;

using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class ClockFunction : ICallable
{
    public int Arity() => 0;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        return (double)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
    }
}
