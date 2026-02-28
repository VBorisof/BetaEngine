using BDSM.Runtime;
using BDSM.Functions;
using BDSM.ExecutionContexts;

using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class RandomFunction : ICallable
{
    public int Arity() => 2;

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        return (double)Random.Shared.Next((int)(double)arguments[0], (int)(double)arguments[1]);
    }
}