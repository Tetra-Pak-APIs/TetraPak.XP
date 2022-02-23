using TetraPak.XP.Auth.Abstractions;
using TetraPak.XP.Auth.Abstractions.OIDC;
using Xunit;

namespace TetraPak.XP.Auth.Tests
{
    public class GrantScopeTests
    {
        [Fact]
        public void Empty()
        {
            var empty = new GrantScope();
            Assert.True(empty.IsEmpty);
            Assert.True(GrantScope.Empty.IsEmpty);
        }

        [Fact]
        public void Unsupported()
        {
            Assert.False(GrantScope.IsScopeSupported("openid profile nope email", out var unsupported));
            Assert.Single(unsupported);
            Assert.Equal("nope", unsupported[0]);
        }

        [Fact]
        public void Single()
        {
            const string Expected = "email";

            var scope = new GrantScope(Expected);
            Assert.Equal(1, scope.Count);
            Assert.Equal(Expected, scope);
            Assert.True(Expected == scope);
            Assert.False(Expected != scope);
            Assert.Equal(Expected, scope.StringValue);
        }

        [Fact]
        public void Multiple()
        {
            const string Expected = "openid email profile";
            
            GrantScope scope = Expected;
            Assert.Equal(3, scope.Count);
            Assert.Contains("openid", scope.Items);
            Assert.Contains("email", scope.Items);
            Assert.Contains("profile", scope.Items);

            var differentOrder = (GrantScope) "email openid profile";
            Assert.Equal(scope, differentOrder);
            Assert.True(scope == differentOrder);
            Assert.False(differentOrder != scope);
        }

        public GrantScopeTests()
        {
            DiscoveryDocument.SetCurrent(new DiscoveryDocument
            { 
                ScopesSupported = new[]
                {
                    "openid",
                    "profile",
                    "email",
                    "general",
                    "groups"
                } 
            });
        }
    }
}