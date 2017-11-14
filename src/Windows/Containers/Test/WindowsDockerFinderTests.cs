// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Registry;
using Microsoft.R.Containers.Docker;
using Microsoft.R.Platform.OS;
using Microsoft.UnitTests.Core.XUnit;
using NSubstitute;

namespace Microsoft.R.Containers.Windows.Test {
    [ExcludeFromCodeCoverage]
    [Category.Threads]
    public class WindowsDockerFinderTests {
        private readonly IServiceManager _services;

        public WindowsDockerFinderTests() {
            _services = new ServiceManager()
                .AddService(GetFileSystemMock())
                .AddService<IProcessServices, WindowsProcessServices>();
        }

        [Test]
        public void FindDockerInstallationTest() {
            _services.AddService(new RegistryMock(GetDefaultInstallKey(), GetServiceInstallKey()));
            RunTest();
        }

        [Test]
        public void FindDockerInstallationUsingServiceTest() {
            _services.AddService(new RegistryMock(GetServiceInstallKey()));
            RunTest();
        }

        [Test]
        public void FindDockerInstallationUsingProgramFilesTest() {
            _services.AddService(new RegistryMock());
            RunTest();
        }

        private void RunTest() {
            var finder = new WindowsLocalDockerFinder(_services);
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
