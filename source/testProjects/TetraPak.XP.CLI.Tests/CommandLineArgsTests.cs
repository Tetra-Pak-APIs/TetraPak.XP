using Xunit;

namespace TetraPak.XP.CLI.Tests
{
    public sealed class CommandLineArgsTests
    {
        [Fact]
        public void Ensure_args_are_correctly_split()
        {
            var args = (CommandLineArgs)"-a \"some text\" -b";
            Assert.Equal(3, args.Count);
            Assert.Equal("-a", args[0]);
            Assert.Equal("some text", args[1]);
            Assert.Equal("-b", args[2]);
        }
    }
}