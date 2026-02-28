using System.Collections.Generic;

namespace Beta.Input;

public class InputContextManager
{
    private readonly Dictionary<string, InputContext> _contexts = [];

    public InputContext GetOrCreateByName(string name)
    {
        var nameLower = name.ToLowerInvariant();

        if (!_contexts.TryGetValue(nameLower, out var context))
        {
            context = new InputContext
            {
                Name = nameLower
            };
            _contexts[nameLower] = context;
            return context;
        }
        return context;
    }
}
