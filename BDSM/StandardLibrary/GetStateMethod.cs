using BDSM.Instances;
using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class GetStateMethod : ICallable
{
    public int Arity() => 0;

    private BDSMActor _caller;

    public GetStateMethod(BDSMActor caller)
    {
        _caller = caller;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        return _caller.State;
    }
}