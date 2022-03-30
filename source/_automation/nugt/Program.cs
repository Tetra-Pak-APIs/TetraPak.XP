using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nugt.policies;
using TetraPak.XP;
using TetraPak.XP.DependencyInjection;
using TetraPak.XP.Desktop;
using TetraPak.XP.Logging;
using TetraPak.XP.Logging.Abstractions;

namespace nugt
{
    static class Program
    {
        const string Executable = "nugt";
        
        const string ArgHelp1 = "-?";
        const string ArgHelp2 = "-h";
        const string ArgHelp3 = "--help";
        
        const string ArgSilent1 = "-s";
        const string ArgSilent2 = "--silent";

        static bool IsInteractive { get; set; }

        static bool IsHelpRequested { get; set; }
        
        static async Task Main(string[] args)
        {
            IsInteractive = !args.TryGetFlag(ArgSilent1, ArgSilent2);
            var info = args.BuildTetraPakDesktopHost(collection =>
            {
                collection.AddSingleton<PolicyDispatcher>();
                if (IsInteractive)
                {
                    collection.AddSingleton(p => new LogBase(p.GetService<IConfiguration>()).WithConsoleLogging());
                }
            });
            var p = info.ServiceServiceCollection.BuildXpServices();
            var policyOutcome = initPolicy(
                args, 
                p.GetRequiredService<PolicyDispatcher>(),
                p.GetService<ILog>());
            
            if (!policyOutcome)
            {
                exitWithOutcome(policyOutcome);
                return;
            }

            var policy = policyOutcome.Value!;
            exitWithOutcome(await policy.RunAsync());
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

        static Outcome<Policy> initPolicy(string[] args, PolicyDispatcher policies, ILog? log)
        {
            if (!args.TryGetFirstValue(out var policyName))
            {
                IsHelpRequested = IsInteractive;
                return Outcome<Policy>.Fail(new CodedException(Errors.MissingArgument,"A policy name was expected (first argument)"));
            }
            
            var outcome = policies.GetPolicyType(policyName);
            if (!outcome)
                return Outcome<Policy>.Fail(outcome.Exception!);

            var policyType = outcome.Value!; 
            var ctor = policyType.GetConstructor(new Type[] { typeof(string[]), typeof(ILog) });
            if (ctor is null)
                return Outcome<Policy>.Fail($"Expected policy ctor with argument types: {typeof(string[]).Name} and {nameof(ILog)}");

            try
            {
                var policy = (Policy)ctor.Invoke(new object?[] { args, log });
                return policy.OutcomeFromInit
                    ? Outcome<Policy>.Success(policy)
                    : Outcome<Policy>.Fail(policy.OutcomeFromInit.Exception!);
            }
            catch (Exception ex)
            {
                return Outcome<Policy>.Fail($"Error when initializing policy \"{policyName}\" ({policyType})");
            }
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
            for (var i = 0; i < args.Count; i++)
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

    sealed class CodedException : Exception
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