// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Registry;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Interpreters.Test {
    [ExcludeFromCodeCoverage]
    [Category.R.Install]
    [Collection(CollectionNames.NonParallel)]
    public class RInstallationTest {
        [CompositeTest]
        [Category.R.Install]
        [InlineData(@"C:\", @"C:\")]
        [InlineData(@"C:\R", @"C:\R")]
        [InlineData(@"C:\R\bin", @"C:\R")]
        [InlineData(@"C:\R\bin\x64", @"C:\R")]
        public void NormalizePath(string path, string expected) {
            RInterpreterInfo.NormalizeRPath(path).Should().Be(expected);
        }

        [Test]
        public void ActualInstall() {
            // Use actual files and registry
            var svr = new SupportedRVersionRange(3, 2, 3, 9);
            var engine = new RInstallation().GetCompatibleEngines(svr).FirstOrDefault();

            engine.Should().NotBeNull();
            engine.Name.Should().NotBeNullOrEmpty();

            engine.Version.Major.Should().BeGreaterOrEqualTo(3);
            engine.Version.Minor.Should().BeGreaterOrEqualTo(2);

            Directory.Exists(engine.InstallPath).Should().BeTrue();
            Directory.Exists(engine.BinPath).Should().BeTrue();

            string path = Path.Combine(engine.InstallPath, @"bin\x64");
            Directory.Exists(path).Should().BeTrue();
        }

        [Test]
        public void Simulate01() {
            var tr = new RegistryMock(SimulateRegistry01());

            string dir = @"C:\Program Files\MRO\R-3.2.3";
            string dir64 = dir + @"\bin\x64\";
            var fs = Substitute.For<IFileSystem>();
            PretendRFilesAvailable(fs, dir);

            var fvi = Substitute.For<IFileVersionInfo>();
            fvi.FileMajorPart.Returns(3);
            fvi.FileMinorPart.Returns(23);
            fs.GetVersionInfo(dir64 + "R.dll").Returns(fvi);

            var ri = new RInstallation(tr, fs);
            var svl = new SupportedRVersionRange(3, 2, 3, 2);

            var engines = ri.GetCompatibleEngines(svl);
            engines.Should().NotBeEmpty();

            var e = engines.FirstOrDefault();
            e.CheckInstallation(svl, fs).Should().BeTrue();
            
            e.Version.Major.Should().BeGreaterOrEqualTo(3);
            e.Version.Minor.Should().BeGreaterOrEqualTo(2);
            e.InstallPath.Should().StartWithEquivalent(@"C:\Program Files");
            e.InstallPath.Should().Contain("R-");
            e.Version.Should().Be(new Version(3, 2, 3));
        }

        private RegistryKeyMock[] SimulateRegistry01() {
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
        public void Simulate02() {
            var tr = new RegistryMock(SimulateRegistry02());
            var svl = new SupportedRVersionRange(3, 2, 3, 9);
            var ri = new RInstallation(tr, null);

            ri.GetCompatibleEngines(svl).Should().BeEmpty();

            string dir = @"C:\Program Files\RRO\R-3.1.3";
            var fs = Substitute.For<IFileSystem>();
            var fsi = Substitute.For<IFileSystemInfo>();
            fsi.Attributes.Returns(FileAttributes.Directory);
            fsi.FullName.Returns(dir);
            fs.GetDirectoryInfo(@"C:\Program Files\RRO").EnumerateFileSystemInfos().Returns(new IFileSystemInfo[] { fsi });
            
            ri = new RInstallation(tr, fs);
            var e = ri.GetCompatibleEngines(svl).FirstOrDefault();
            e.Should().NotBeNull();
            e.Version.Should().Be(new Version(3, 1, 3));
            e.Name.Should().Be("R-3.1.3");

            PretendRFilesAvailable(fs, dir);

            var coreShell = Substitute.For<ICoreShell>();
            e.CheckInstallation(svl, fs, coreShell, showErrors: true).Should().BeFalse();
            coreShell.When(x => x.ShowErrorMessage(Arg.Any<string>())).Do(x => {
                var s = x.Args()[0] as string;
                s.Should().Contain("not compatible");
            });
        }

        [Test]
        public void Simulate03() {
            var tr = new RegistryMock(SimulateRegistry02());

            string dir = @"C:\Program Files\RRO\R-3.1.3";
            var fs = Substitute.For<IFileSystem>();
            PretendRFilesAvailable(fs, dir);

            var fvi = Substitute.For<IFileVersionInfo>();
            fvi.FileMajorPart.Returns(3);
            fvi.FileMinorPart.Returns(13);
            fs.GetVersionInfo(dir + "R.dll").Returns(fvi);

            var ri = new RInstallation(tr, fs);
            var svl = new SupportedRVersionRange(3, 2, 3, 2);

            var e = ri.GetCompatibleEngines().FirstOrDefault();
            e.Should().NotBeNull();

            var coreShell = Substitute.For<ICoreShell>();
            e.CheckInstallation(svl, fs, coreShell, showErrors: true).Should().BeFalse();
            coreShell.When(x => x.ShowErrorMessage(Arg.Any<string>())).Do(x => {
                var s = x.Args()[0] as string;
                s.Should().Contain("Cannot find");
            });
        }

        private RegistryKeyMock[] SimulateRegistry02() {
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
