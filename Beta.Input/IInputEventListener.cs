using System.Collections.Generic;

namespace Beta.Input;

public interface IInputEventListener
{
    public HashSet<InputContext> GetInputContexts();
    public InputEventConsumeResult OnInputEvent(InputEventArgs args);
}
