#nullable disable

using System;

namespace BDSM.ExecutionContexts;

public class ExecutionContext
{
    public static ExecutionContext Shared { get; } = new(ExecutionContextType.Shared);
    public static ExecutionContext Async()
    {
        return new ExecutionContext(ExecutionContextType.Async)
        {
            AsyncTag = Guid.NewGuid()
        };
    }

    public Guid AsyncTag { get; set; }

    public string ActorName { get; set; }
    public static ExecutionContext Actor(string actorName)
    {
        return new ExecutionContext(ExecutionContextType.Actor)
        {
            ActorName = actorName
        };
    }

    public ExecutionContextType ContextType { get; set; }

    public ExecutionContext(ExecutionContextType contextType)
    {
        ContextType = contextType;
    }
}