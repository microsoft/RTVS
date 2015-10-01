using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test.Project
{
    public class FileSystemMirroringProjectUtilitiesTest
    {
        [CompositeTest]
        [InlineData(@"C:\Temp\abc.csproj", @"c:\temp\")]
        [InlineData(@"C:/Temp/abc.csproj", @"c:\temp\")]
        public void GetProjectDirectory(string fullPath, string expected)
        {
            var project = Substitute.For<UnconfiguredProject>();
            project.FullPath.Returns(fullPath);
            project.GetProjectDirectory().Should().BeEquivalentTo(expected);
        }

        [CompositeTest]
        [InlineData(@"C:\Temp\abc.csproj", @"abc.InMemory.Targets")]
        [InlineData(@"abc.def.csproj", @"abc.def.InMemory.Targets")]
        public void GetInMemoryTargetsFileName(string filename, string expected)
        {
            var actual = FileSystemMirroringProjectUtilities.GetInMemoryTargetsFileName(filename);
            actual.Should().BeEquivalentTo(expected);
        }

        [CompositeTest]
        [InlineData(@"C:\Temp\abc.csproj", @"c:\temp\abc.InMemory.Targets")]
        [InlineData(@"C:\Temp\abc.def.csproj", @"c:\temp\abc.def.InMemory.Targets")]
        [InlineData(@"C:/Temp/abc.csproj", @"c:\temp\abc.InMemory.Targets")]
        public void GetInMemoryTargetsFileFullPath(string fullPath, string expected)
        {
            var project = Substitute.For<UnconfiguredProject>();
            project.FullPath.Returns(fullPath);
            project.GetInMemoryTargetsFileFullPath().Should().BeEquivalentTo(expected);
        }
    }
}
