using System;

namespace Beta.Input;

public record InputTransformer
{
    public required Func<InputEventArgs, InputEventArgs> Transform { get; init; }
}
