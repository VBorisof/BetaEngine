using System;
using System.Collections.Generic;
using Beta.Common;

namespace Beta.DI;

public class DependencyContainer : Singleton<DependencyContainer>
{
    private readonly Dictionary<Type, object> _objects = [];

    public void Clear()
    {
        _objects.Clear();
    }

    public void Add<T>(object t)
    {
        try
        {
            var typed = (T)t;
            _objects.Add(typeof(T), t);
        }
        catch
        {
            throw new DependencyException(
                $"Object {t} cannot be cast to type {typeof(T)}"
            );
        }

    }

    public T Get<T>()
    {
        if (!_objects.TryGetValue(typeof(T), out var result))
        {
            throw new DependencyException(
                $"Failed to resolve dependency on type {typeof(T)}: "
                + "Object not registered"
            );
        }

        try
        {
            var typedResult = (T)result;
            return typedResult;
        }
        catch
        {
            throw new DependencyException(
                $"Failed to resolve dependency on type {typeof(T)}: "
                + "Failed to cast an object to desired type."
            );
        }
    }
}