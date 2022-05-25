using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nugt.policies;
using TetraPak.XP;
using TetraPak.XP.ApplicationInformation;
using TetraPak.XP.CLI;
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

        const string ArgLogFile1 = "-lf";
        const string ArgLogFile2 = "--log-file";

        static bool IsInteractive { get; set; }

        static bool IsHelpRequested { get; set; }
        
        static CommandLineArgs? Args { get; set; }

        static ILog? s_log;
        static readonly List<LogEventArgs> s_logEvents = new();

        static async Task Main(string[] args)
        {
            Args = new CommandLineArgs(args);
            IsInteractive = !args.TryGetFlag(ArgSilent1, ArgSilent2);
            var info = args.BuildTetraPakDesktopHost(ApplicationFramework.Console, collection =>
            {
                collection.AddSingleton<PolicyDispatcher>();
                collection.AddSingleton(p => new LogBase(p.GetRequiredService<IConfiguration>()).WithConsoleLogging());
                if (IsInteractive)
                {
                    collection.AddSingleton(p =>
                    {
                        var log = new LogBase(p.GetService<IConfiguration>()).WithConsoleLogging();
                        log.Logged += (_, e) => s_logEvents.Add(e);
                        return log;
                    });
                }
            });
            var provider = info.ServiceCollection.BuildXpServices();
            s_log = provider.GetService<ILog>();
            var policyOutcome = initPolicy(
                Args,
                provider.GetRequiredService<PolicyDispatcher>(),
                provider.GetService<ILog>());

            if (!policyOutcome)
            {
                await exitWithOutcome(policyOutcome);
                return;
            }

            var policy = policyOutcome.Value!;
            await exitWithOutcome(await policy.RunAsync());
        }

        static async Task exitWithOutcome(Outcome outcome)
        {
            writeToLog(outcome);
            if (outcome)
            {
                Environment.Exit(0);
                return;
            }

            var code = outcome.Exception is CodedException codedException ? codedException.Code : 1;
            if (IsHelpRequested)
            {
                showHelp();
            }

            if (!s_logEvents.Any())
                Environment.Exit(code);
                
            var logFilePath = Args!.TryGetValue(out var lfp, ArgLogFile1, ArgLogFile2)
                ? lfp
                : Path.Combine(Environment.CurrentDirectory, "_fail.log");
            
            var sb = new StringBuilder();
            foreach (var logEvent in s_logEvents)
            {
                sb.AppendLine(logEvent.Format());
            }

            try
            {
                await File.WriteAllTextAsync(logFilePath, sb.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;  
            }
            
            Environment.Exit(code);
        }

        static void showHelp()
        {
            writeToConsole("TODO: Add help");
        }

        static Outcome<Policy> initPolicy(CommandLineArgs args, PolicyDispatcher policies, ILog? log)
        {
            if (!args.TryGetFirstValue(out var policyName))
            {
                IsHelpRequested = IsInteractive;
                return Outcome<Policy>.Fail(new CodedException(Errors.MissingArgument,
                    "A policy name was expected (first argument)"));
            }

            var outcome = policies.GetPolicyType(policyName);
            if (!outcome)
                return Outcome<Policy>.Fail(outcome.Exception!);

            var policyType = outcome.Value!;
            var ctor = policyType.GetConstructor(new[] { typeof(CommandLineArgs), typeof(ILog) });
            if (ctor is null)
                return Outcome<Policy>.Fail(
                    $"Expected policy ctor with argument types: {nameof(CommandLineArgs)} and {nameof(ILog)}");

            try
            {
                var policy = (Policy)ctor.Invoke(new object?[] { args, log });
                return policy.OutcomeFromInit
                    ? Outcome<Policy>.Success(policy)
                    : Outcome<Policy>.Fail(policy.OutcomeFromInit.Exception!);
            }
            catch (Exception ex)
            {
                ex = new Exception($"Error when initializing policy \"{policyName}\" ({policyType})", ex);
                return Outcome<Policy>.Fail(ex);
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

        static void writeToLog(Outcome outcome)
        {
            if (!IsInteractive)
                return;
        
            if (outcome)
            {
                s_log.Information("SUCCESS");
                return;
            }
            
            s_log.Error(outcome.Exception!);
        }
    }
}