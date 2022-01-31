using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using prepNuget.strategies;
using TetraPak.XP;
using TetraPak.XP.Logging;

namespace prepNuget
{
    class Program
    {
        const string Executable = "prepnuget";
        
        const string ArgHelp1 = "-?";
        const string ArgHelp2 = "-h";
        const string ArgHelp3 = "--help";
        
        const string ArgSilent1 = "-s";
        const string ArgSilent2 = "--silent";

        static bool IsInteractive { get; set; }

        static bool IsHelpRequested { get; set; }
        
        static void Main(string[] args)
        {
            var strategyOutcome = initStrategy(args);
            if (!strategyOutcome)
            {
                exitWithOutcome(strategyOutcome);
                return;
            }
        }
        
        static void exitWithOutcome(Outcome outcome)
        {
            if (outcome)
            {
                writeToConsole(outcome);
                Environment.Exit(0);
                return;
            }

            
            var code = outcome.Exception is CodedException codedException ? codedException.Code : 1;
            writeToConsole(outcome);
            if (IsHelpRequested)
            {
                showHelp();
            }
            Environment.Exit(code);
        }

        static void showHelp()
        {
            writeToConsole("TODO: Add help");
        }

        static Outcome<NugetStrategy> initStrategy(string[] args)
        {
            IsInteractive = !args.TryGetFlag(ArgSilent1, ArgSilent2);
            if (args.TryGetFirstValue(out var strategy))
                return NugetStrategy.Select(strategy,  args,new BasicLog().WithConsoleLogging());
            
            IsHelpRequested = IsInteractive;
            return Outcome<NugetStrategy>.Fail(new CodedException(Errors.MissingArgument,"You must specify a strategy (first argument)"));
        }
        
        static void writeToConsole(string message, ConsoleColor? color = ConsoleColor.Yellow)
        {
            if (!IsInteractive) 
                return;

            Console.ForegroundColor = color ?? ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void writeToConsole(Outcome outcome, ConsoleColor? color = null)
        {
            if (!IsInteractive) 
                return;

            if (outcome)
            {
                Console.ForegroundColor = color ?? ConsoleColor.Green;
                Console.WriteLine("SUCCESS");
                Console.ResetColor();
                return;
            }
            
            Console.ForegroundColor = color ?? ConsoleColor.Red;
            Console.WriteLine(outcome.Exception!.Message);
            Console.ResetColor();
        }
    }

    public static class ArgsHelper
    {
        internal static bool TryGetValue(this IReadOnlyList<string> args, [NotNullWhen(true)] out string? value, params string[] keys)
        {
            for (var i = 0; i < args.Count-1; i++)
            {
                if (!keys.Any(key => key.Equals(args[i], StringComparison.Ordinal))) 
                    continue;
                
                value = args[i + 1];
                return true;
            }

            value = null;
            return false;
        }
        
        internal static bool TryGetFlag(this IReadOnlyList<string> args, params string[] keys)
        {
            for (var i = 0; i < args.Count-1; i++)
            {
                if (keys.Any(key => key.Equals(args[i], StringComparison.Ordinal))) 
                    return true;
            }
            return false;
        }
        
        internal static bool TryGetFirstValue(this IReadOnlyList<string> args, [NotNullWhen(true)] out string? value)
        {
            value = args.Count >= 1 ? args[0] : null;
            return value is {};
        }
    }

    class CodedException : Exception
    {
        public int Code { get; }

        public CodedException(int code, string message, Exception? innerException = null) : base(message, innerException)
        {
            Code = code;
        }
    }

    static class Errors
    {
        public const int MissingArgument = 10;
        public const int InvalidArgument = 20;
    }
    
}