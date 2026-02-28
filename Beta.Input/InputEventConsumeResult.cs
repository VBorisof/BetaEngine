namespace Beta.Input;

public readonly record struct InputEventConsumeResult(bool swallowEvent = false, bool markEvent = false)
{
    public bool SwallowEvent { get; } = swallowEvent;
    public bool MarkEvent { get; } = markEvent;
}
