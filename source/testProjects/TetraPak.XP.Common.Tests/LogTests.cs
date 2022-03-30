using System;
using System.Net;
using TetraPak.XP.Logging;
using Xunit;

namespace TetraPak.XP.Common.Tests
{
    public sealed class LogTests
    {
        [Fact]
        public void RequestTest()
        {
            var r = (HttpWebRequest) WebRequest.Create("https://call.me/please");
            r.Headers.Add("x-one", "value-one");
            var log = new LogBase().WithConsoleLogging();
            log.DebugWebRequest(r, "hello world!");
            var text = log.ToString();
            var nl = Environment.NewLine;
            Assert.Equal($@"GET https://call.me/please{nl}x-one=value-one{nl}{nl}hello world!{nl}", text);
        }
    }
}