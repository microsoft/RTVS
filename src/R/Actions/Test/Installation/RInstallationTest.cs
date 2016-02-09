using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.R.Actions.Utility;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Actions.Test.Installation {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class RInstallationTest {
        [Test]
        [Category.R.Install]
        public void RInstallation_Test01() {
            RInstallData data = RInstallation.GetInstallationData(null, 0, 0, 0, 0);
            Assert.True(data.Status == RInstallStatus.PathNotSpecified || data.Status == RInstallStatus.UnsupportedVersion);
        }

        [Test]
        [Category.R.Install]
        public void RInstallation_Test02() {
            // Use actual files and registry
            RInstallation.Registry = null;
            RInstallation.FileSystem = null;

            RInstallData data = RInstallation.GetInstallationData(null, 3, 2, 3, 2);
            data.Status.Should().Be(RInstallStatus.OK);
            data.Version.Major.Should().BeGreaterOrEqualTo(3);
            data.Version.Minor.Should().BeGreaterOrEqualTo(2);
            data.Path.Should().StartWithEquivalent(@"C:\Program Files");
            data.Path.Should().Contain("R-");
        }

        [Test]
        [Category.R.Install]
        public void RInstallation_Test03() {
            var tr = new RegistryMock(SimulateRegistry03());
            RInstallation.Registry = tr;

            string dir = @"C:\Program Files\MRO\R-3.2.3";
            string dir64 = dir + @"\bin\x64\";
            var fs = Substitute.For<IFileSystem>();
            PretendRFilesAvailable(fs, dir);

            var fvi = Substitute.For<IFileVersionInfo>();
            fvi.FileMajorPart.Returns(3);
            fvi.FileMinorPart.Returns(23);
            fs.GetVersionInfo(dir64 + "R.dll").Returns(fvi);

            RInstallation.FileSystem = fs;
            RInstallData data = RInstallation.GetInstallationData(null, 3, 2, 3, 2);

            data.Status.Should().Be(RInstallStatus.OK);
            data.Version.Major.Should().BeGreaterOrEqualTo(3);
            data.Version.Minor.Should().BeGreaterOrEqualTo(2);
            data.Path.Should().StartWithEquivalent(@"C:\Program Files");
            data.Path.Should().Contain("R-");
            data.Version.Should().Be(new Version(3, 2, 3));
        }

        private RegistryKeyMock[] SimulateRegistry03() {
            return new RegistryKeyMock[] {
                new RegistryKeyMock(
                     @"SOFTWARE\R-core\R",
                     new RegistryKeyMock[] {
                            new RegistryKeyMock("3.2.3"),
                            new RegistryKeyMock("3.2.2"),
                            new RegistryKeyMock("8.0.3")
                     }),
                new RegistryKeyMock(
                     @"SOFTWARE\R-core\R\3.2.3",
                     new RegistryKeyMock[0],
                     new string[] {"InstallPath"},
                     new string[] { @"C:\Program Files\MRO\R-3.2.3" }),
            };
        }

        [Test]
        [Category.R.Install]
        public void RInstallation_Test04() {
            var tr = new RegistryMock(SimulateRegistry04());
            RInstallation.Registry = tr;

            RInstallation.GetCompatibleEnginePathFromRegistry().Should().BeNullOrEmpty();

            string dir = @"C:\Program Files\RRO\R-3.1.3";
            var fs = Substitute.For<IFileSystem>();
            var fsi = Substitute.For<IFileSystemInfo>();
            fsi.Attributes.Returns(System.IO.FileAttributes.Directory);
            fsi.FullName.Returns(dir);
            fs.GetDirectoryInfo(@"C:\Program Files\RRO").EnumerateFileSystemInfos().Returns(new IFileSystemInfo[] { fsi });
            RInstallation.FileSystem = fs;

            RInstallData data = RInstallation.GetInstallationData(null, 3, 2, 3, 2);
            data.Status.Should().Be(RInstallStatus.PathNotSpecified);

            PretendRFilesAvailable(fs, dir);
            data = RInstallation.GetInstallationData(dir, 3, 2, 3, 2);
            data.Status.Should().Be(RInstallStatus.UnsupportedVersion);
        }

        [Test]
        [Category.R.Install]
        public void RInstallation_Test05() {
            var tr = new RegistryMock(SimulateRegistry04());
            RInstallation.Registry = tr;

            string dir = @"C:\Program Files\RRO\R-3.1.3";
            var fs = Substitute.For<IFileSystem>();
            PretendRFilesAvailable(fs, dir);

            var fvi = Substitute.For<IFileVersionInfo>();
            fvi.FileMajorPart.Returns(3);
            fvi.FileMinorPart.Returns(13);
            fs.GetVersionInfo(dir + "R.dll").Returns(fvi);

            RInstallation.FileSystem = fs;
            RInstallData data = RInstallation.GetInstallationData(dir, 3, 2, 3, 2);
            data.Status.Should().Be(RInstallStatus.UnsupportedVersion);
        }

        private RegistryKeyMock[] SimulateRegistry04() {
            return new RegistryKeyMock[] {
                new RegistryKeyMock(
                     @"SOFTWARE\R-core\R",
                     new RegistryKeyMock[] {
                             new RegistryKeyMock("8.0.3")
                     }),
             };
        }

        private void PretendRFilesAvailable(IFileSystem fs, string dir) {
            string dir64 = dir + @"\bin\x64\";
            fs.DirectoryExists(dir).Returns(true);
            fs.DirectoryExists(dir64).Returns(true);
            fs.FileExists(dir64 + "R.dll").Returns(true);
            fs.FileExists(dir64 + "Rgraphapp.dll").Returns(true);
            fs.FileExists(dir64 + "RTerm.exe").Returns(true);
            fs.FileExists(dir64 + "RScript.exe").Returns(true);
            fs.FileExists(dir64 + "RGui.exe").Returns(true);
        }
    }
}
