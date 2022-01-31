using System.Linq;
using TetraPak.XP.Auth;
using Xunit;

namespace TetraPak.Auth.Xamarin.Tests
{
    public class AuthScopeTests
    {
        [Fact]
        public void Empty()
        {
            var empty = new AuthScope();
            Assert.True(empty.IsEmpty);
            Assert.True(AuthScope.Empty.IsEmpty);
        }

        [Fact]
        public void Unsupported()
        {
            Assert.False(AuthScope.IsScopeSupported("nope", out var unsupported));
            Assert.Equal("nope", unsupported);
        }

        [Fact]
        public void Single()
        {
            const string Expected = "email";

            var scope = new AuthScope(Expected);
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
            
            AuthScope scope = Expected;
            Assert.Equal(3, scope.Count);
            Assert.Contains("openid", scope.Items);
            Assert.Contains("email", scope.Items);
            Assert.Contains("profile", scope.Items);

            var differentOrder = (AuthScope) "email openid profile";
            Assert.Equal(scope, differentOrder);
            Assert.True(scope == differentOrder);
            Assert.False(differentOrder != scope);
        }
    }
}