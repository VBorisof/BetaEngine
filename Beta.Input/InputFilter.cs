using System;

namespace Beta.Input;

public record InputFilter
{
    public required Func<InputEventArgs, bool> Filter { get; init; }
}
