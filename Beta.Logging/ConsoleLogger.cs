using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Beta.Common.Extensions;

namespace Beta.Logging;

public class ConsoleLogger : ILogger
{
    private LogLevel _logLevel;

    public ConsoleLogger(LogLevel logLevel)
    {
        _logLevel = logLevel;
    }

    public void Error(string message="")
    {
        if (_logLevel < LogLevel.Error) return;

        var st = new StackTrace(fNeedFileInfo: true);
        var frame = st.GetFrame(1);
        (string filename, string methodname) = GetFileAndMethodNames(frame);
        ConsoleEx.WriteLine(
            $"[ERR] {filename}::{methodname}: {message}",
            ConsoleColor.Red
        );
    }

    public void Warning(string message="")
    {
        if (_logLevel < LogLevel.Warning) return;

        var st = new StackTrace(fNeedFileInfo: true);
        var frame = st.GetFrame(1);
        (string filename, string methodname) = GetFileAndMethodNames(frame);
        ConsoleEx.WriteLine(
            $"[WRN] {filename}::{methodname}: {message}",
            ConsoleColor.Yellow
        );
    }

    public void Info(string message="")
    {
        if (_logLevel < LogLevel.Info) return;

        var st = new StackTrace(fNeedFileInfo: true);
        var frame = st.GetFrame(1);
        (string filename, string methodname) = GetFileAndMethodNames(frame);
        ConsoleEx.WriteLine(
            $"[INF] {filename}::{methodname}: {message}",
            ConsoleColor.White
        );
    }

    public void Debug(string message="")
    {
        if (_logLevel < LogLevel.Debug) return;

        var st = new StackTrace(fNeedFileInfo: true);
        var frame = st.GetFrame(1);
        (string filename, string methodname) = GetFileAndMethodNames(frame);
        ConsoleEx.WriteLine(
            $"[DBG] {filename}::{methodname}: {message}",
            ConsoleColor.DarkMagenta
        );
    }

    public void Trace(string message="")
    {
        if (_logLevel < LogLevel.Trace) return;

        var st = new StackTrace(fNeedFileInfo: true);
        var frame = st.GetFrame(1);
        (string filename, string methodname) = GetFileAndMethodNames(frame);
        ConsoleEx.WriteLine(
            $"[TRC] {filename}::{methodname}: {message}",
            ConsoleColor.DarkYellow
        );
    }

    public void LogMore()
    {
        if (_logLevel >= LogLevel.Trace)
        {
            _logLevel = LogLevel.Trace;
            return;
        }
        ++_logLevel;
    }

    public void LogLess()
    {
        if (_logLevel <= LogLevel.Error)
        {
            _logLevel = LogLevel.Error;
            return;
        }
        --_logLevel;
    }

    private (string, string) GetFileAndMethodNames(StackFrame? frame)
    {
        if (frame is null)
        {
            return ("", "");
        }
        var filename = frame.GetFileName()?.Split(Path.DirectorySeparatorChar).Last();
        if (filename is null)
        {
            filename = "";
        }
        var methodname = frame.GetMethod()?.Name;
        if (methodname is null)
        {
            methodname = "";
        }

        return (filename, methodname);
    }
}