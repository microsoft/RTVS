// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.R.Common.Core.Output;
using Microsoft.R.Containers.Docker;
using Microsoft.UnitTests.Core.XUnit;
using System.IO;
using System;
using Microsoft.R.Platform.IO;
using Microsoft.R.Platform.OS;
using NSubstitute;

namespace Microsoft.R.Containers.Windows.Test {
    [ExcludeFromCodeCoverage]
    [Category.Threads]
    public class WindowsContainerTests {
        private readonly IServiceManager _services;

        public WindowsContainerTests() {
            _services = new ServiceManager()
                .AddService<IFileSystem, WindowsFileSystem>()
                .AddService<IProcessServices, WindowsProcessServices>()
                .AddService<IRegistry, RegistryImpl>()
                .AddService(Substitute.For<IActionLog>())
                .AddService<IOutputService, TestOutputService>();
        }

        [Test]
        public async Task BuildImageTest() {
            var dockerFileContent = @"FROM ubuntu:16.10
RUN apt-get update && apt-get upgrade -y";
            var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirectory);
            var dockerFile = Path.Combine(tempDirectory, "Dockerfile");
            File.WriteAllText(dockerFile, dockerFileContent);
            var svc = new WindowsDockerService(_services);
            var param = new BuildImageParameters(dockerFile, "rtvs-test-build-image", "latest", "mycontainer", 5444);
            await svc.CreateContainerFromFileAsync(param, CancellationToken.None);
            Directory.Delete(tempDirectory, true);
            await svc.DeleteContainerAsync("mycontainer", CancellationToken.None);
        }

        [Test]
        public async Task CreateAndDeleteContainerTest() {
            var svc = new WindowsDockerService(_services);
            var param = new ContainerCreateParameters("docker.io/kvnadig/rtvs-linux", "latest");
            var container = await svc.CreateContainerAsync(param, CancellationToken.None);
            var container2 = await svc.GetContainerAsync(container.Id, CancellationToken.None);
            container.Id.Should().Be(container2.Id);
            await svc.DeleteContainerAsync(container.Id, CancellationToken.None);
            var containers = await svc.ListContainersAsync(true, CancellationToken.None);
            containers.Should().NotContain(c => c.Id == container.Id);
        }

        [Test]
        public async Task StartStopContainerTest() {
            var svc = new WindowsDockerService(_services);
            var param = new ContainerCreateParameters("docker.io/kvnadig/rtvs-linux", "latest");
            var container = await svc.CreateContainerAsync(param, CancellationToken.None);
            await svc.StartContainerAsync(container.Id, CancellationToken.None);

            var runningContainers = await svc.ListContainersAsync(false, CancellationToken.None);
            runningContainers.Should().Contain(c => c.Id == container.Id);

            await svc.StopContainerAsync(container.Id, CancellationToken.None);

            var runningContainers2 = await svc.ListContainersAsync(false, CancellationToken.None);
            runningContainers2.Should().NotContain(c => c.Id == container.Id);

            await svc.DeleteContainerAsync(container.Id, CancellationToken.None);
            var allContainers = await svc.ListContainersAsync(true, CancellationToken.None);
            allContainers.Should().NotContain(c => c.Id == container.Id);
        }

        [Test]
        public async Task CleanImageDownloadTest() {
            var svc = new WindowsDockerService(_services);

            var param = new ContainerCreateParameters("hello-world", "latest");
            string imageName = $"{param.Image}:{param.Tag}";
            await DeleteImageAsync(imageName);

            var images = await svc.ListImagesAsync(false, CancellationToken.None);
            images.Should().NotContain(c => c.Name == param.Image && c.Tag == param.Tag);

            var container = await svc.CreateContainerAsync(param, CancellationToken.None);
            await svc.StartContainerAsync(container.Id, CancellationToken.None);

            var images2 = await svc.ListImagesAsync(false, CancellationToken.None);
            images2.Should().Contain(c => c.Name == param.Image && c.Tag == param.Tag);

            await svc.StopContainerAsync(container.Id, CancellationToken.None);
            await svc.DeleteContainerAsync(container.Id, CancellationToken.None);
            var allContainers = await svc.ListContainersAsync(true, CancellationToken.None);
            allContainers.Should().NotContain(c => c.Id == container.Id);

            await DeleteImageAsync(imageName);
        }

        private async Task<bool> DeleteImageAsync(string image) {
            var psi = new ProcessStartInfo {
                FileName = "docker",
                Arguments = $"rmi -f {image}",
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(psi);
            process.WaitForExit();

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            return !string.IsNullOrEmpty(error);
        }
    }
}
