using BDSM.Logging;
using BDSM.Runtime;
using System;

namespace BDSM;

public class Program
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        if (args.Length > 1)
        {
            Console.WriteLine("[!] Usage: bdsm [script].");
            System.Environment.Exit(69);
        }

        var driver = new Driver(BDSMLogLevel.Error);
        var success = true;
        if (args.Length == 1)
        {
            driver.SetRootDir(System.Environment.CurrentDirectory);
            driver.RunFileFromRelativePath(args[0]);
        }
        else
        {
            driver.RunPrompt();
        }

        if (!success)
        {
            System.Environment.Exit(65);
        }
    }
}