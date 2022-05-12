using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TetraPak.XP.Logging.Abstractions;
using Xunit;

namespace TetraPak.XP.Logging.Tests
{
    public sealed class LogSectionTests
    {
        [Fact]
        public void Test_that_retained_section_events_are_written_together()
        {
            var events = new List<string>();
            var log = new LogBase(LogRank.Trace).WithTestLogging(events);
            var tasks = new[]
            {
                Task.Run(() =>
                {
                    for (var i = 0; i < 70; i++)
                    {
                        log.Information($"event#{i + 1}");
                        Task.Delay(10);
                    }
                }),
                Task.Run(() =>
                {
                    Task.Delay(30);
                    using var section = log.Section("TEST SECTION", sectionSuffix:"--END--");
                    for (var i = 0; i < 10; i++)
                    {
                        section.Debug($"sub-event#{i+1}");
                        Task.Delay(10);
                    } 
                })
            };
            Task.WaitAll(tasks);
            
            // ensure the section events are not interrupted by other events ...
            var sectionStart = events.IndexOf(message => message.Contains("TEST SECTION"));
            Assert.NotEqual(-1, sectionStart);
            var i = 0;
            string e;
            for (i+=1; i < 10; i++)
            {
                e = events[sectionStart + i];
                Assert.Contains("sub-event#", e, StringComparison.Ordinal);
            }

            e = events[sectionStart + i + 1];
            Assert.Contains("--END--", e);
        }
    }

    static  class TestLogHelper 
    {
        internal static ILog WithTestLogging(this LogBase log, List<string> events)
        {
            log.Logged += (_, e) =>
            {
                events.Add(e.Format());
            };
            return log;
        }
    }
}