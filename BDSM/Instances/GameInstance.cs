using System;
using System.Collections.Generic;
using System.Linq;
using BDSM.ExecutionContexts;
using BDSM.Language;
using BDSM.Runtime;
using BDSM.StandardLibrary;
using Environment = BDSM.Runtime.Environment;

#nullable disable

namespace BDSM.Instances;

public class GameInstance : Instance
{
    public string DeclName { get; }
    public List<VerbStatement> Verbs { get; }

    public GameInstance(string declName, List<VerbStatement> verbs)
    {
        DeclName = declName;
        Verbs = verbs;

        AddMethod("playanimation", new PlayAnimationMethod(this));
        AddMethod("setpos", new SetPosMethod(this));
    }

    public void ExecuteVerb(Interpreter interpreter, string verbName, ExecutionContext context)
    {
        var verb = Verbs.SingleOrDefault(v => v.name.Lexeme.Equals(verbName, StringComparison.OrdinalIgnoreCase));

        if (verb != null)
        {
            var env = new Environment(interpreter.Globals);
            interpreter.ExecuteBlock(verb.statements, env, context);
        }
    }
}