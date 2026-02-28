using System.Collections.Generic;

namespace BDSM.Utilities;

#nullable disable

internal static class SizeCalculator
{
    private static readonly HashSet<object> _visitedObjects = [];

    public static int GetSizeOf(object obj)
    {
        _visitedObjects.Clear();
        return CalculateSize(obj);
    }

    private static int CalculateSize(object obj)
    {
        // TODO: fix for AOT.
        return -1;

        /*
        if (obj == null || _visitedObjects.Contains(obj))
        {
            return 0;
        }

        _visitedObjects.Add(obj);

        var size = 0;

        if (obj is Enum)
        {
            size += Enum.GetValues(obj.GetType()).Length;
        }
        else if (obj is ICollection collection)
        {
            foreach (var element in collection)
            {
                size += CalculateSize(element);
            }
        }
        else if (obj.GetType().IsValueType && !obj.GetType().IsGenericType)
        {
            size += Marshal.SizeOf(obj);
        }
        else
        {
            foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                size += CalculateSize(field.GetValue(obj));
            }
        }

        return size;
        */
    }
}