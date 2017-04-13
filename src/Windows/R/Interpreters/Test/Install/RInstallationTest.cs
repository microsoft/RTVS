// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Registry;
using Microsoft.Common.Core.UI;
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

            var fvi = new Version(3, 23);
            fs.GetFileVersion(dir64 + "R.dll").Returns(fvi);

            var ri = new RInstallation(tr, fs);
            var svl = new SupportedRVersionRange(3, 2, 3, 2);

            var engines = ri.GetCompatibleEngines(svl);
            engines.Should().NotBeEmpty();

            var e = engines.FirstOrDefault();
            e.VerifyInstallation(svl).Should().BeTrue();
            
            e.Version.Major.Should().BeGreaterOrEqualTo(3);
            e.Version.Minor.Should().BeGreaterOrEqualTo(2);
            e.InstallPath.Should().StartWithEquivalent(@"C:\Program Files");
            e.InstallPath.Should().Contain("R-");
            e.Version.Should().Be(new Version(3, 2, 3));
        }

        private RegistryKeyMock[] SimulateRegistry01() {
            return new[] {
                new RegistryKeyMock(@"SOFTWARE\R-core\R64",
                    new RegistryKeyMock(@"3.2.3",
                            new RegistryKeyMock[0],
                            new[] {"InstallPath"},
                            new[] { @"C:\Program Files\MRO\R-3.2.3" }),
                    new RegistryKeyMock("3.2.2"),
                    new RegistryKeyMock("8.0.3")
                )
            };
        }

        [Test]
        public void IncompatibleVerson01() {
            var tr = new RegistryMock(SimulateRegistry02());
            var svl = new SupportedRVersionRange(3, 2, 3, 9);
            var fs = Substitute.For<IFileSystem>();
            var ri = new RInstallation(tr, fs);

            ri.GetCompatibleEngines(svl).Should().BeEmpty();

            string dir = @"C:\Program Files\RRO\R-3.1.3";
            var fsi = Substitute.For<IFileSystemInfo>();
            fsi.Attributes.Returns(FileAttributes.Directory);
            fsi.FullName.Returns(dir);
            fs.GetDirectoryInfo(@"C:\Program Files\RRO").EnumerateFileSystemInfos().Returns(new[] { fsi });
            
            ri = new RInstallation(tr, fs);
            var engines = ri.GetCompatibleEngines(svl);
            engines.Should().BeEmpty();
        }

        [Test]
        public void IncompatibleVerson02() {
            var tr = new RegistryMock(SimulateRegistry02());
            var svl = new SupportedRVersionRange(3, 1, 3, 9);

            string dir = @"C:\Program Files\R\R-3.1.3";
            var fs = Substitute.For<IFileSystem>();
            var fsi = Substitute.For<IFileSystemInfo>();
            fsi.Attributes.Returns(FileAttributes.Directory);
            fsi.FullName.Returns(dir);
            fs.GetDirectoryInfo(@"C:\Program Files\R").EnumerateFileSystemInfos().Returns(new IFileSystemInfo[] { fsi });

            var fvi = new Version(3, 13);
            fs.GetFileVersion(Path.Combine(dir, @"bin\x64", "R.dll")).Returns(fvi);

            PretendRFilesAvailable(fs, dir);
            var ri = new RInstallation(tr, fs);

            var engines = ri.GetCompatibleEngines(svl);
            var e = engines.FirstOrDefault();
            e.Should().NotBeNull();

            svl = new SupportedRVersionRange(3, 2, 3, 9);
            var coreShell = Substitute.For<ICoreShell>();

            e = new RInterpreterInfo(e.Name, e.InstallPath, fs);
            e.VerifyInstallation(svl, new ServiceManager().AddService(coreShell)).Should().BeFalse();
            coreShell.When(x => x.ShowMessage(Arg.Any<string>(), MessageButtons.OK)).Do(x => {
                var s = x.Args()[0] as string;
                s.Should().Contain("not compatible");
            });
            coreShell.Received().ShowMessage(Arg.Any<string>(), MessageButtons.OK);
        }


        [Test]
        public void IncompatibleVersonInPF() {
            var tr = new RegistryMock(SimulateRegistry02());
            var svl = new SupportedRVersionRange(3, 1, 3, 9);

            var root = @"C:\Program Files\R";
            string dir = Path.Combine(root, "R-3.1.3");
            var fs = Substitute.For<IFileSystem>();
            fs.DirectoryExists(root).Returns(true);
            fs.DirectoryExists(dir).Returns(true);

            var fsi = Substitute.For<IFileSystemInfo>();
            fsi.Attributes.Returns(FileAttributes.Directory);
            fsi.FullName.Returns(dir);
            fs.GetDirectoryInfo(root).EnumerateFileSystemInfos().Returns(new[] { fsi });

            var ri = new RInstallation(tr, fs);
            var engines = ri.GetCompatibleEngines(svl);
            engines.Should().BeEmpty();
        }

        [Test]
        public void MissingBinaries() {
            var tr = new RegistryMock(SimulateRegistry02());
            var svl = new SupportedRVersionRange(3, 1, 3, 4);

            string dir = @"C:\Program Files\R\R-3.1.3";
            var fs = Substitute.For<IFileSystem>();
            var fsi = Substitute.For<IFileSystemInfo>();
            fsi.Attributes.Returns(FileAttributes.Directory);
            fsi.FullName.Returns(dir);
            fs.GetDirectoryInfo(@"C:\Program Files\R").EnumerateFileSystemInfos().Returns(new[] { fsi });

            var fvi = new Version(3, 13);
            fs.GetFileVersion(Path.Combine(dir, @"bin\x64", "R.dll")).Returns(fvi);

            PretendRFilesAvailable(fs, dir);
            var ri = new RInstallation(tr, fs);

            var e = ri.GetCompatibleEngines(svl).FirstOrDefault();
            e.Should().NotBeNull();

            fs = Substitute.For<IFileSystem>();
            e = new RInterpreterInfo(e.Name, e.InstallPath, fs);
            var coreShell = Substitute.For<ICoreShell>();
            e.VerifyInstallation(svl, new ServiceManager().AddService(coreShell)).Should().BeFalse();

            coreShell.When(x => x.ShowMessage(Arg.Any<string>(), MessageButtons.OK)).Do(x => {
                var s = x.Args()[0] as string;
                s.Should().Contain("Cannot find");
            });
            coreShell.Received().ShowMessage(Arg.Any<string>(), MessageButtons.OK);
        }

        [Test]
        public void Duplicates() {
            var tr = new RegistryMock(SimulateDuplicates());
            var svl = new SupportedRVersionRange(3, 2, 3, 4);

            string dir = @"C:\Program Files\Microsoft\R Client\R_SERVER";
            var fs = Substitute.For<IFileSystem>();
            var fsi = Substitute.For<IFileSystemInfo>();
            fsi.Attributes.Returns(FileAttributes.Directory);
            fsi.FullName.Returns(dir);
            fs.GetDirectoryInfo(@"C:\Program Files\Microsoft\R Client\R_SERVER").EnumerateFileSystemInfos().Returns(new[] { fsi });

            var fvi = new Version(3, 22);
            fs.GetFileVersion(Path.Combine(dir, @"bin\x64", "R.dll")).Returns(fvi);

            PretendRFilesAvailable(fs, dir);
            var ri = new RInstallation(tr, fs);

            var engines = ri.GetCompatibleEngines(svl);
            engines.Should().HaveCount(1);

            var e = engines.First();
            e.Name.Should().Contain("Microsoft R");
            e = new RInterpreterInfo(e.Name, e.InstallPath, fs);
        }
        
        private RegistryKeyMock[] SimulateRegistry02() {
            return new[] {
                new RegistryKeyMock(
                     @"SOFTWARE\R-core\R64", new RegistryKeyMock(
                         @"3.1.3",
                         new RegistryKeyMock[0],
                         new[] {"InstallPath"},
                         new[] { @"C:\Program Files\R\R-3.1.3" })),
            };
        }

        private RegistryKeyMock[] SimulateDuplicates() {
            return new[] {
                new RegistryKeyMock(
                     @"SOFTWARE\R-core\R64", 
                     new RegistryKeyMock(@"3.2.2.803",
                         new RegistryKeyMock[0],
                         new[] {"InstallPath"},
                         new[] { @"C:\Program Files\Microsoft\R Client\R_SERVER\" }),
                     new RegistryKeyMock(@"3.2.2.803 Microsoft R Client",
                         new RegistryKeyMock[0],
                         new[] {"InstallPath"},
                         new[] { @"C:\Program Files\Microsoft\R Client\R_SERVER" })),
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
