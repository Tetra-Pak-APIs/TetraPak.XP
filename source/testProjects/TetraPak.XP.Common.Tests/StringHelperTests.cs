using System;
using Xunit;

namespace TetraPak.XP.Common.Tests
{
    public sealed class StringHelperTests
    {
        [Fact]
        public void ContainsTests()
        {
            const string A = "aBcDeFgHiJ";
            Assert.False(A.Contains("ghij", StringComparison.Ordinal));
            Assert.True(A.Contains("gHiJ", StringComparison.Ordinal));
            Assert.True(A.Contains("ghij", StringComparison.OrdinalIgnoreCase));
        }
    }
}