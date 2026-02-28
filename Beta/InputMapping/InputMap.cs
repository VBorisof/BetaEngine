using Beta.Input;
using System;

namespace Beta.InputMapping;

public class InputMap
{
    public GameInputType GameInputType { get; }
    private readonly Predicate<InputEventArgs> _matchingEventPredicate;

    public InputMap(GameInputType inputType, Predicate<InputEventArgs> matchingEventPredicate)
    {
        GameInputType = inputType;
        _matchingEventPredicate = matchingEventPredicate;
    }

    public bool IsMatch(InputEventArgs inputArgs)
    {
        return _matchingEventPredicate.Invoke(inputArgs);
    }
}
