using System;

namespace Beta.Common.Extensions;

public static class ConsoleEx
{
    public static void WriteLine(string input, ConsoleColor color)
    {
        var originalFg = Console.ForegroundColor;
        Console.ForegroundColor = color;

        Console.WriteLine(input);

        Console.ForegroundColor = originalFg;
    }

    public static void Write(string input, ConsoleColor color)
    {
        var originalFg = Console.ForegroundColor;
        Console.ForegroundColor = color;

        Console.Write(input);

        Console.ForegroundColor = originalFg;
    }
}


