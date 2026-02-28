using BDSM.Language;
using BDSM.Logging;
using BDSM.Parsing;
using BDSM.Runtime.SaveGames;
using BDSM.Scanning;
using Beta.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace BDSM.Runtime;

public class Driver
{
    private readonly IBDSMLogger _logger;
    private readonly Scanner _scanner;
    private readonly Parser _parser;
    public Interpreter Interpreter { get; }
    public string RootDir { get; private set; } = "";
    private string _source = "";

    public Driver(BDSMLogLevel logLevel = BDSMLogLevel.None)
    {
        _logger = new ConsoleBDSMLogger(logLevel);
        _scanner = new Scanner(_logger);
        _parser = new Parser(_logger);
        Interpreter = new Interpreter(_logger);
        Interpreter.RuntimeError += (_, err) => OnRuntimeError(err);
    }

    public Driver()
    {
        _logger = new DummyBDSMLogger();
        _scanner = new Scanner(_logger);
        _parser = new Parser(_logger);
        Interpreter = new Interpreter(_logger);
        Interpreter.RuntimeError += (_, err) => OnRuntimeError(err);
    }

    public void SetRootDir(string rootDir)
    {
        RootDir = rootDir;
        Interpreter.RootDir = rootDir;
    }

    public bool RunFileFromRelativePath(string relativePath)
    {
        var path = Path.Combine(RootDir, relativePath);
        _logger.Debug($"Run file from {path}...");
        var success = Run(FileLoader.ReadAllFromFile(path));

        return success;
    }
    public bool RunFileFromAbsolutePath(string path)
    {
        _logger.Debug($"Run file {path}...");
        var success = Run(FileLoader.ReadAllFromFile(path));

        return success;
    }

    public bool RunPrompt()
    {
        bool isRunning = true;
        ConsoleCancelEventHandler onCtrlC = (_, __) =>
        {
            isRunning = false;
            _logger.Info("[+] Exit. Bye!");
        };

        Console.CancelKeyPress += onCtrlC;

        Console.WriteLine(FileLoader.ReadAllFromFile("info.txt"));
        while (isRunning)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line != null)
            {
                Run(line);
            }
        }

        Console.CancelKeyPress -= onCtrlC;

        return true;
    }

    public bool Run(string source)
    {
        _source = source;
        var tokens = _scanner.ScanTokens(_source);

        _logger.Debug($"TOKENS: \n  {string.Join("\n  ", tokens)}");

        if (!_scanner.IsSuccess)
        {
            _logger.Info("Please fix syntax errors.");
            return false;
        }

        try
        {
            var expression = _parser.Parse(tokens);
            if (expression == null)
            {
                return false;
            }

            Interpreter.Interpret(expression);
        }
        catch (ParseError)
        {
            _logger.Info("Please fix parsing errors.");
        }

        return true;
    }

    public BDSMSaveData GetBDSMSaveData()
    {
        return Interpreter.Globals.GetBDSMSaveData();
    }

    public void SetBDSMSaveData(BDSMSaveData data)
    {
        Interpreter.Globals.SetBDSMSaveData(data);
    }

    public List<Statement?>? ScanAndParse(string source)
    {
        var tokens = _scanner.ScanTokens(source);

        _logger.Debug($"TOKENS: \n  {string.Join("\n  ", tokens)}");

        if (!_scanner.IsSuccess)
        {
            _logger.Info("Please fix syntax errors.");
            return null;
        }

        try
        {
            var expression = _parser.Parse(tokens);
            if (expression == null)
            {
                return null;
            }
            return expression;
        }
        catch (ParseError)
        {
            _logger.Info("Please fix parsing errors.");
        }

        return null;
    }

    private void OnRuntimeError(RuntimeError error)
    {
        var lines = _source.Split('\n');

        if (error.Token != null)
        {
            try
            {
                _logger.Error(
                    error.Token.Line,
                    error.Token.Column,
                    lines[Math.Max(0, error.Token.Line - 1)],
                    $"{error.Token.Lexeme}:\n    {error.Message}"
                );
            }
            catch (IndexOutOfRangeException)
            {
                // TODO: Fix the error reporting.
                // We're storing the source weirdly hence
                // we're sometimes in inconsistent state.
                _logger.Error(
                    1, 1, "", error.Message
                );
            }
        }
        else
        {
            _logger.Error(
                1, 1, "", error.Message
            );
        }
    }
}