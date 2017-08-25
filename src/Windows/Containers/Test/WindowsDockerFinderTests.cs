// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Registry;
using Microsoft.Common.Core.Test.StubBuilders;
using Microsoft.R.Containers.Docker;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;

namespace Microsoft.R.Containers.Windows.Test {
    [ExcludeFromCodeCoverage]
    [Category.Threads]
    public class WindowsDockerFinderTests {
        [Test]
        public void FindDockerInstallationTest() {
            var services = new ServiceManager()
                .AddService(GetFileSystemMock())
                .AddService<IProcessServices, ProcessServices>()
                .AddService(new RegistryMock(GetDefaultInstallKey(), GetServiceInstallKey()));

            var finder = new WindowsLocalDockerFinder(services);
            var docker = finder.GetLocalDocker();
            docker.Should().NotBeNull();
            docker.BinPath.Should().NotBeEmpty();
            docker.DockerCommandPath.Should().NotBeEmpty();
        }

        [Test]
        public void FindDockerInstallationUsingServiceTest() {
            var services = new ServiceManager()
                .AddService(GetFileSystemMock())
                .AddService<IProcessServices, ProcessServices>()
                .AddService(new RegistryMock(GetServiceInstallKey()));

            var finder = new WindowsLocalDockerFinder(services);
            var docker = finder.GetLocalDocker();
            docker.Should().NotBeNull();
            docker.BinPath.Should().NotBeEmpty();
            docker.DockerCommandPath.Should().NotBeEmpty();
        }

        [Test]
        public void FindDockerInstallationUsingProgramFilesTest() {
            var services = new ServiceManager()
                .AddService(GetFileSystemMock())
                .AddService<IProcessServices, ProcessServices>()
                .AddService(new RegistryMock());

            var finder = new WindowsLocalDockerFinder(services);
            var docker = finder.GetLocalDocker();
            docker.Should().NotBeNull();
            docker.BinPath.Should().NotBeEmpty();
            docker.DockerCommandPath.Should().NotBeEmpty();
        }

        private RegistryKeyMock GetDefaultInstallKey() {
            string[] valueNames = { "BinPath", "Version" };
            string[] values = { "\"C:\\Program Files\\Docker\\Docker\\resources\\bin\"" , "1.0.0.0" };
            var v1 = new RegistryKeyMock("1.0", null, valueNames, values);
            return new RegistryKeyMock(@"SOFTWARE\Docker Inc.\Docker", v1);
        }

        private RegistryKeyMock GetServiceInstallKey() {
            string[] valueNames = { "ImagePath" };
            string[] values = { "\"C:\\Program Files\\Docker\\Docker\\com.docker.service\"" };
            return new RegistryKeyMock(@"SYSTEM\CurrentControlSet\Services\com.docker.service", null, valueNames, values);
        }

        private IFileSystem GetFileSystemMock() {
            string path = @"C:\Program Files\Docker\Docker\resources\bin\docker.exe";
            var fs = Substitute.For<IFileSystem>();
            fs.FileExists(path).Returns(true);
            return fs;
        }
    }
}
