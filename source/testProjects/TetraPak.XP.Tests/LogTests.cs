using System;
using System.Net;
using TetraPak.XP.Logging;
using Xunit;

namespace TetraPak.Auth.Xamarin.Tests
{
    public class LogTests
    {
        [Fact]
        public void RequestTest()
        {
            var r = (HttpWebRequest) WebRequest.Create("https://call.me/please");
            r.Headers.Add("x-one", "value-one");
            var log = new BasicLog().WithConsoleLogging();
            log.DebugWebRequest(r, "hello world!");
            var text = log.ToString();
            var nl = Environment.NewLine;
            Assert.Equal($@"GET https://call.me/please{nl}x-one=value-one{nl}{nl}hello world!{nl}", text);
        }
    }
    
    // class TestLog : ILog
    // {
    //     readonly StringBuilder _sb = new StringBuilder(); obsolete
    //     
    //     public event EventHandler<LogEventArgs> Logged;
    //     
    //     public LogQueryAsyncDelegate LogQueryAsync { get; set; }
    //     
    //     public void Write(LogRank logRank, string? message = null, Exception? exception = null)
    //     {
    //         if (exception is { })
    //         {
    //             _sb.AppendLine(exception.ToString());
    //             return;
    //         }
    //         _sb.AppendLine(message);
    //         if (message is {})
    //             _sb.AppendLine();
    //     }
    //
    //     public override string ToString() => _sb.ToString();
    // }
}