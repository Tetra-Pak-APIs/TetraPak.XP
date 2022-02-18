using System;
using TetraPak.XP;
using TetraPak.XP.Auth;
using TetraPak.XP.Auth.Abstractions;
using Xunit;

namespace TetraPak.Auth.Xamarin.Tests
{
    public class AuthConfigTests
    {
        [Fact]
        public void InitialState()
        {
            var config = getStartConfig();
            Assert.True(string.IsNullOrEmpty(config.Scope));
        }

        [Fact]
        public void AddScope()
        {
            var config = getStartConfig(new GrantScope(" aaa ", " bbb "));
            Assert.Equal("aaa bbb", config.Scope);

            config.AddScope((GrantScope) " bbb ccc ");
            Assert.Equal("aaa bbb ccc", config.Scope);
        }
        
        [Fact]
        public void RemoveScope()
        {
            var config = getStartConfig(new GrantScope(" aaa ", " bbb "));
            config.RemoveScope("bbb");
            Assert.Equal("aaa", config.Scope);
            config.RemoveScope("ccc");
            Assert.Equal("aaa", config.Scope);
            config.RemoveScope("aaa");
            Assert.True(string.IsNullOrEmpty(config.Scope));
        }

        static AuthConfig getStartConfig(GrantScope? scope = null)
        {
            return AuthConfig
                .Default(RuntimeEnvironment.Migration, "clientId", new Uri("test://redirect"), null!)
                .WithScope(scope);
        }
    }
}