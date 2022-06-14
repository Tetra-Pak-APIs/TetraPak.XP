using System.IO;
using TetraPak.XP.ProjectManagement;
using Xunit;

namespace TetraPak.XP.Nuget
{
    public sealed class NugetVersionTests
    {
        [Fact]
        public void Ensure_parsing_production_version()
        {
            var nv = (NugetVersion)"1.0.0";
            Assert.Equal(1, nv.Major);
            Assert.Equal(0, nv.Minor);
            Assert.Equal(0, nv.Revision);
            Assert.False(nv.IsPrerelease);
            Assert.Null(nv.Prerelease);
        }
        
        [Fact]
        public void Ensure_parsing_implicit_prerelease_version()
        {
            var nv = (NugetVersion)"1.0.0-alpha";
            Assert.Equal(1, nv.Major);
            Assert.Equal(0, nv.Minor);
            Assert.Equal(0, nv.Revision);
            Assert.True(nv.IsPrerelease);
            Assert.NotNull(nv.Prerelease);
            Assert.Equal(ProjectPhase.Alpha, nv.Prerelease!.Phase);
            Assert.Equal(1, nv.Prerelease.Version);
        }
        
        [Fact]
        public void Ensure_parsing_explicit_prerelease_version()
        {
            var nv = (NugetVersion)"1.0.1-beta.2";
            Assert.Equal(1, nv.Major);
            Assert.Equal(0, nv.Minor);
            Assert.Equal(1, nv.Revision);
            Assert.True(nv.IsPrerelease);
            Assert.NotNull(nv.Prerelease);
            Assert.Equal(ProjectPhase.Beta, nv.Prerelease!.Phase);
            Assert.Equal(2, nv.Prerelease.Version);
        }
        
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