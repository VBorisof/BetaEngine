using BDSM.Instances;
using BDSM.ExecutionContexts;
using BDSM.Functions;
using BDSM.Runtime;
using System;
using System.Collections.Generic;

#nullable disable

namespace BDSM.StandardLibrary;

public class SceneRemoveMethod : ICallable
{
    public int Arity() => 1;

    private BDSMScene _scene;

    public SceneRemoveMethod(BDSMScene scene)
    {
        _scene = scene;
    }

    public object Call(Interpreter interpreter, List<object> arguments, ExecutionContext context)
    {
        try {
            interpreter.EventHandlers.OnSceneRemove(
                this,
                new SceneRemoveEventArgs(
                    context,
                    _scene,
                    (GameInstance) arguments[0]
                )
            );
        }
        catch (InvalidCastException)
        {
            throw new RuntimeError(
                null,
                $"{_scene.DeclName}.add expects an actor or item."
            );
        }
        return null;
    }
}


