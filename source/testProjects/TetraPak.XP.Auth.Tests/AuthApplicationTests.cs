using System;
using System.Runtime.InteropServices;
using Xunit;

namespace TetraPak.XP.Auth.Tests
{
    public class AuthApplicationTests
    {
        [Fact]
        public void Minimum()
        {
            var redirectUri = new Uri("myapp://test");
            var app = (AuthApplication) $"12345, {redirectUri}";
            Assert.Equal("12345", app.ClientId);
            Assert.Equal(redirectUri, app.RedirectUri);
            Assert.Equal(RuntimeEnvironment.Production, app.Environment);
            Assert.Equal(RuntimePlatform.Any, app.RuntimePlatform);
        }
        
        [Fact]
        public void WithEnvironment()
        {
            var redirectUri = new Uri("myapp://test");
            var app = (AuthApplication) $"DEV, 12345, {redirectUri}";
            Assert.Equal("12345", app.ClientId);
            Assert.Equal(redirectUri, app.RedirectUri);
            Assert.Equal(RuntimeEnvironment.Development, app.Environment);
            Assert.Equal(RuntimePlatform.Any, app.RuntimePlatform);
        }

        [Fact]
        public void WithPlatform()
        {
            var redirectUri = new Uri("myapp://test");
            var app = (AuthApplication) $"iOS, 12345, {redirectUri}";
            Assert.Equal("12345", app.ClientId);
            Assert.Equal(redirectUri, app.RedirectUri);
            Assert.Equal(RuntimeEnvironment.Production, app.Environment);
            Assert.Equal(RuntimePlatform.IOS, app.RuntimePlatform);
        }
        
        [Fact]
        public void WithEverything()
        {
            var redirectUri = new Uri("myapp://test");
            var app = (AuthApplication) $"iOS, TEST, 12345, {redirectUri}";
            Assert.Equal("12345", app.ClientId);
            Assert.Equal(redirectUri, app.RedirectUri);
            Assert.Equal(RuntimeEnvironment.Test, app.Environment);
            Assert.Equal(RuntimePlatform.IOS, app.RuntimePlatform);
        }
    }
}