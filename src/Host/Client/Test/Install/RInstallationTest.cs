// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Common.Core.Test.Registry;
using Microsoft.R.Host.Client.Install;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Host.Client.Test.Install {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class RInstallationTest {
        [Test]
        [Category.R.Install]
        public void Test01() {
            var svl = new SupportedRVersionRange(0, 0, 0, 0);
            RInstallData data = RInstallation.GetInstallationData(null, svl);
            data.Status.Should().BeEither(RInstallStatus.PathNotSpecified, RInstallStatus.UnsupportedVersion);
        }

        [CompositeTest]
        [Category.R.Install]
        [InlineData(@"C:\", @"C:\")]
        [InlineData(@"C:\R", @"C:\R")]
        [InlineData(@"C:\R\bin", @"C:\R")]
        [InlineData(@"C:\R\bin\x64", @"C:\R")]
        public void NormalizePath(string path, string expected) {
            RInstallation.NormalizeRPath(path).Should().Be(expected);
        }

        [Test]
        [Category.R.Install]
        public void Test02() {
            // Use actual files and registry
            RInstallation.Registry = null;
            RInstallation.FileSystem = null;

            var svl = new SupportedRVersionRange(3, 2, 3, 2);
            RInstallData data = RInstallation.GetInstallationData(null, svl);
            data.Status.Should().Be(RInstallStatus.OK);
            data.Version.Major.Should().BeGreaterOrEqualTo(3);
            data.Version.Minor.Should().BeGreaterOrEqualTo(2);
            string path = Path.Combine(data.Path, @"bin\x64");
            Directory.Exists(path).Should().BeTrue();
        }

        [Test]
        [Category.R.Install]
        public void Test03() {
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
            var svl = new SupportedRVersionRange(3, 2, 3, 2);
            RInstallData data = RInstallation.GetInstallationData(null, svl);

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
        public void Test04() {
            var tr = new RegistryMock(SimulateRegistry04());
            RInstallation.Registry = tr;
            var svl = new SupportedRVersionRange(3, 2, 3, 9);

            RInstallation.GetCompatibleEnginePathFromRegistry(svl).Should().BeNullOrEmpty();

            string dir = @"C:\Program Files\RRO\R-3.1.3";
            var fs = Substitute.For<IFileSystem>();
            var fsi = Substitute.For<IFileSystemInfo>();
            fsi.Attributes.Returns(System.IO.FileAttributes.Directory);
            fsi.FullName.Returns(dir);
            fs.GetDirectoryInfo(@"C:\Program Files\RRO").EnumerateFileSystemInfos().Returns(new IFileSystemInfo[] { fsi });
            RInstallation.FileSystem = fs;

            RInstallData data = RInstallation.GetInstallationData(null, svl);
            data.Status.Should().Be(RInstallStatus.PathNotSpecified);

            PretendRFilesAvailable(fs, dir);
            data = RInstallation.GetInstallationData(dir, svl);
            data.Status.Should().Be(RInstallStatus.UnsupportedVersion);
        }

        [Test]
        [Category.R.Install]
        public void Test05() {
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
            var svl = new SupportedRVersionRange(3, 2, 3, 2);
            RInstallData data = RInstallation.GetInstallationData(dir, svl);
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

        [Test]
        [Category.R.Install]
        public void MsRClient() {
            var rClientInstallPath = @"C:\Program Files\Microsoft\R Client\";
            var rClientRPath = @"C:\Program Files\Microsoft\R Client\R_SERVER\";
            var tr = new RegistryMock(SimulateRegistryMsRClient(rClientInstallPath, rClientRPath));
            RInstallation.Registry = tr;

            RInstallation.GetRClientPath().Should().Be(rClientRPath);

            var shell = Substitute.For<ICoreShell>();
            shell.ShowMessage(Arg.Any<string>(), Arg.Any<MessageButtons>()).Returns(MessageButtons.Yes);

            RInstallation.GetRInstallPath(null, null, shell).Should().Be(rClientRPath);
            shell.Received(1).ShowMessage(Arg.Any<string>(), Arg.Any<MessageButtons>());

            RInstallation.GetRInstallPath(null, null, shell).Should().Be(rClientRPath);
            shell.Received(1).ShowMessage(Arg.Any<string>(), Arg.Any<MessageButtons>());
        }

        private RegistryKeyMock[] SimulateRegistryMsRClient(string rClientInstallPath, string rClientRPath) {
            return new RegistryKeyMock[] {
                new RegistryKeyMock(
                        name: @"SOFTWARE\Microsoft\R Client",
                        subkeys: null,
                        valueNames: new string[] {"Path"}, 
                        values: new string[] {rClientInstallPath}),
                new RegistryKeyMock(
                        name: @"SOFTWARE\Microsoft\R Tools\" + Toolset.Version),
                new RegistryKeyMock(
                        name: @"SOFTWARE\Microsoft\Microsoft SQL Server\130\sql_shared_mr",
                        subkeys: null,
                        valueNames: new string[] {"Path"},
                        values: new string[] {rClientRPath}),
                new RegistryKeyMock(
                       @"SOFTWARE\R-core\R64",
                        new RegistryKeyMock[] {
                            new RegistryKeyMock(
                                name: "3.2.5",
                                subkeys: null,
                                valueNames: new string[] {"InstallPath"},
                                values: new string[] {rClientRPath}),
                        })
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
