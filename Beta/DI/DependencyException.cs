using System;

namespace Beta.DI;

public class DependencyException : Exception
{
    public DependencyException(string message) : base(message) { }
}
