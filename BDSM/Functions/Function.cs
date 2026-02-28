using System.Collections.Generic;
using BDSM.Runtime;
using BDSM.ExecutionContexts;
using BDSM.Language;

#nullable disable

namespace BDSM.Functions;

public class Function : ICallable
{
    private readonly FunctionStatement _declaration;

    public Function(FunctionStatement declaration)
    {
        _declaration = declaration;
    }


    public int Arity()
    {
        return _declaration.parameters.Count;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        var env = new Environment(interpreter.Globals);
        for (var i = 0; i < _declaration.parameters.Count; ++i)
        {
            env.Define(_declaration.parameters[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(_declaration.body, env, context);
        }
        catch (Return r)
        {
            return r.Value;
        }
        return null;
    }

    public override string ToString()
    {
        return $"<{_declaration.name.Lexeme}>";
    }
}