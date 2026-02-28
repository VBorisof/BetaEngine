namespace Beta.Actors;

public class ActorState
{
    public string Name { get; set; }
    public bool IsManuallyManaged { get; set; }

    public static ActorState Idle { get; } = new("idle");
    public static ActorState Talk { get; } = new("talk");

    public static ActorState PlayAnimation { get; } = new("play-animation");

    public static ActorState WalkNorth { get; } = new("walk_north");
    public static ActorState WalkNorthEast { get; } = new("walk_north");
    public static ActorState WalkEast { get; } = new("walk_north");
    public static ActorState WalkSouthEast { get; } = new("walk_south");
    public static ActorState WalkSouth { get; } = new("walk_south");
    public static ActorState WalkSouthWest { get; } = new("walk_south");
    public static ActorState WalkWest { get; } = new("walk_south");
    public static ActorState WalkNorthWest { get; } = new("walk_north");

    public ActorState(string name, bool isManuallyManaged = false)
    {
        Name = name;
        IsManuallyManaged = isManuallyManaged;
    }
}