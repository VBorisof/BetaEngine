namespace Beta.Input;

internal readonly record struct ModifyInputListenersArgs
{
    public required IInputEventListener Listener { get; init; }
}
