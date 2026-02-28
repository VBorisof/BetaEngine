using Beta.Common.Extensions;
using System;

namespace BDSM.Logging;

public class DummyBDSMLogger : IBDSMLogger
{
    public void Debug(string message)
    {
    }

    public void Error(int lineNumber, int? colNumber, string line, string message)
    {
    }

    public void Info(string message)
    {
    }
}

public class ConsoleBDSMLogger : IBDSMLogger
{
    private readonly BDSMLogLevel _level;

    public ConsoleBDSMLogger(BDSMLogLevel level)
    {
        _level = level;
    }

    public void Error(int lineNumber, int? colNumber, string line, string message)
    {
        if (_level < BDSMLogLevel.Error) return;

        ConsoleEx.WriteLine($"Error:\n", ConsoleColor.Red);
        var marginColumn = $"    {lineNumber} | ";
        ConsoleEx.Write(marginColumn, ConsoleColor.Yellow);
        ConsoleEx.WriteLine(line, ConsoleColor.White);
        if (colNumber != null)
        {
            Console.Write(new string(' ', marginColumn.Length));
            ConsoleEx.WriteLine(
                new string('~', colNumber.Value - 1) + '^',
                ConsoleColor.Yellow
            );
        }
        ConsoleEx.WriteLine($"    {message}\n", ConsoleColor.Red);
    }

    public void Info(string message)
    {
        if (_level < BDSMLogLevel.Info) return;

        ConsoleEx.WriteLine($"Info:\n", ConsoleColor.DarkYellow);
        ConsoleEx.WriteLine($"    {message}", ConsoleColor.Yellow);
    }

    public void Debug(string message)
    {
        if (_level < BDSMLogLevel.Debug) return;

        ConsoleEx.WriteLine($"[D]: {message}\n", ConsoleColor.Gray);
    }
}