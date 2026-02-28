using BDSM.ExecutionContexts;
using BDSM.Runtime;
using System.Collections.Generic;

namespace BDSM.Functions;

public interface ICallable
{
    int Arity();

    object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context);
}