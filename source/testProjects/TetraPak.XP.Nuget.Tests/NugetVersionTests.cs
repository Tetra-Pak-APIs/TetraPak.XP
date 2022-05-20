using System.IO;
using TetraPak.XP.ProjectManagement;
using Xunit;

namespace TetraPak.XP.Nuget
{
    public sealed class NugetVersionTests
    {
        [Fact]
        public void Ensure_comparison_works()
        {
            var a = (NugetVersion)"1.0.0-alpha.2";
            Assert.True("1.0.0-alpha.1" < a);
            Assert.True(a > "1.0.0-alpha.1");
            Assert.True(a < "1.0.0-beta.1");
            Assert.True(a < "1.0.1-alpha.1");
            Assert.True(a < "1.1.0-alpha.1");
            Assert.True(a < "2.0.0-alpha.1");
            Assert.False(a < "1.0.0-alpha.1");
            Assert.True(a >= "1.0.0-alpha.1");
            Assert.True((NugetVersion)"1.0.0-alpha.1" <= "1.0.0-alpha.1");
        }

        [Fact]
        public void Ensure_adjusting_version()
        {
            var a = (NugetVersion)"1.0.0-alpha.2";
            Assert.Equal(((NugetVersion)"0.0.0-alpha.+1").Adjust("1.0.0-alpha.1"), a);
            var b = ((NugetVersion)"1.0.0-alpha.1").Adjust("1.0.0-alpha.2");
            Assert.Equal("1.0.0-alpha.2", b);
            b = ((NugetVersion)"1.0.0-alpha.1").Adjust("1.0.0-alpha.2", VersioningPolicy.Hard);
            Assert.Equal("1.0.0-alpha.1", b);
            b = ((NugetVersion)"1.0.0-alpha.2").Adjust("1.0.0-beta.1");
            Assert.Equal("1.0.0-beta.1", b);
        }

        [Fact]
        public void Ensure_correct_nuget_version_is_written_to_code()
        {
            var fileName = typeof(NugetVersionTests).Assembly.GetName().Name;
            Assert.NotNull(fileName);
            var file = new FileInfo($"../../{fileName}.csproj");
            var projectFile = new ProjectFile(file, true);
        }
    }
}