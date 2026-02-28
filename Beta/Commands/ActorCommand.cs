using Beta.Actors;

namespace Beta.Commands;

public abstract class ActorCommand : Command
{
    public Actor Actor { get; set; }

    public ActorCommand(Actor actor)
    {
        Actor = actor;
    }
}
