// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.R.Containers.Docker;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Containers.Windows.Test {
    [ExcludeFromCodeCoverage]
    [Category.Threads]
    public class WindowsDockerFinderTests {
        [Test]
        public void FindDockerInstallationTest() {
            IServiceManager services= new ServiceManager()
                .AddService<IFileSystem, FileSystem>()
                .AddService<IProcessServices, ProcessServices>()
                .AddService<IRegistry, RegistryImpl>();
            var finder = new WindowsLocalDockerFinder(services);
            var docker = finder.GetLocalDocker();
            docker.Should().NotBeNull();
            docker.BinPath.Should().NotBeEmpty();
            docker.DockerCommandPath.Should().NotBeEmpty();
        }

        [Test]
        public void FindDockerInstallationUsingServiceTest() {
            IServiceManager services = new ServiceManager()
                .AddService<IFileSystem, FileSystem>()
                .AddService<IProcessServices, ProcessServices>()
                .AddService(new RegistryMock(new string[] { WindowsLocalDockerFinder.DockerRegistryPath }));

            var finder = new WindowsLocalDockerFinder(services);
            var docker = finder.GetLocalDocker();
            docker.Should().NotBeNull();
            docker.BinPath.Should().NotBeEmpty();
            docker.DockerCommandPath.Should().NotBeEmpty();
        }

        [Test]
        public void FindDockerInstallationUsingProgramFilesTest() {
            IServiceManager services = new ServiceManager()
                .AddService<IFileSystem, FileSystem>()
                .AddService<IProcessServices, ProcessServices>()
                .AddService(new RegistryMock(new string[] { WindowsLocalDockerFinder.DockerRegistryPath, WindowsLocalDockerFinder.DockerRegistryPath2 }));

            var finder = new WindowsLocalDockerFinder(services);
            var docker = finder.GetLocalDocker();
            docker.Should().NotBeNull();
            docker.BinPath.Should().NotBeEmpty();
            docker.DockerCommandPath.Should().NotBeEmpty();
        }
    }
}
