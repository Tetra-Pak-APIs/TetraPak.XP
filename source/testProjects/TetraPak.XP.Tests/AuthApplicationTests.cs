using System;
using TetraPak.XP.Auth;
using Xunit;

namespace TetraPak.Auth.Xamarin.Tests
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
            Assert.Equal(RuntimePlatform.iOS, app.RuntimePlatform);
        }
        
        [Fact]
        public void WithEverything()
        {
            var redirectUri = new Uri("myapp://test");
            var app = (AuthApplication) $"iOS, TEST, 12345, {redirectUri}";
            Assert.Equal("12345", app.ClientId);
            Assert.Equal(redirectUri, app.RedirectUri);
            Assert.Equal(RuntimeEnvironment.Test, app.Environment);
            Assert.Equal(RuntimePlatform.iOS, app.RuntimePlatform);
        }
    }
}