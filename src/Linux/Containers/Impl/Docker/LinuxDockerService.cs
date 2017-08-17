// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using static System.FormattableString;
namespace Microsoft.R.Containers.Docker {
    public class LinuxDockerService : LocalDockerService, IContainerService {
        const string DockerCePackageName = "docker-ce";
        const string DockerEePackageName = "docker-ee";
        const string DockerProcessName = "dockerd";
        private LocalDocker _docker;
        private readonly IServiceContainer _services;

        public LinuxDockerService(IServiceContainer services) : base(services) {
            _services = services;
        }

        public ContainerServiceStatus GetServiceStatus() {
            var proc = GetDockerProcess();
            if (proc.HasExited) {
                return new ContainerServiceStatus(false, Resources.Error_ServiceNotAvailable, ContainerServiceStatusType.Error);
            } else {
                return new ContainerServiceStatus(true, Resources.Info_ServiceAvailable, ContainerServiceStatusType.Information);
            }
        }

        internal static Process GetDockerProcess() {
            var processes = Process.GetProcessesByName(DockerProcessName);
            return processes.Any() ? processes.FirstOrDefault() : null;
        }

        public async Task<IContainer> CreateContainerAsync(ContainerCreateParameters createParams, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            if (createParams.ImageSourceCredentials != null) {
                await RepositoryLoginAsync(createParams.ImageSourceCredentials, ct);
            }
            try {
                await PullImageAsync(Invariant($"{createParams.Image}:{createParams.Tag}"), ct);

                string createOptions = Invariant($"{createParams.StartOptions} {createParams.Image}:{createParams.Tag} {createParams.Command}");
                var containerId = await CreateContainerAsync(createOptions, ct);
                if (string.IsNullOrEmpty(containerId)) {
                    throw new ContainerException(Resources.Error_ContainerIdInvalid.FormatInvariant(containerId));
                }
                return await GetContainerAsync(containerId, ct);
            } catch (ContainerException cex) {
                throw cex;
            } finally {
                if (createParams.ImageSourceCredentials != null) {
                    await RepositoryLogoutAsync(createParams.ImageSourceCredentials, ct);
                }
            }
        }

        async Task IContainerService.DeleteContainerAsync(IContainer container, CancellationToken ct) {
            var result = await DeleteContainerAsync(container, ct);
            if (!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerDeleteFailed.FormatInvariant(container.Id, result));
            }
        }

        async Task IContainerService.StartContainerAsync(IContainer container, CancellationToken ct) {
            var result = await StartContainerAsync(container, ct);
            if (!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerStartFailed.FormatInvariant(container.Id, result));
            }
        }

        async Task IContainerService.StopContainerAsync(IContainer container, CancellationToken ct) {
            var result = await StopContainerAsync(container, ct);
            if (!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerStopFailed.FormatInvariant(container.Id, result));
            }
        }

        protected override LocalDocker GetLocalDocker() {
            _docker = _docker ?? GetLocalDocker(_services);
            return _docker;
        }

        private static LocalDocker GetLocalDocker(IServiceContainer services) {
            var fs = services.FileSystem();
            const string dockerPath = "/usr/bin/docker";
            var packages = InstalledPackageInfo.GetPackages(fs);
            var dockerPkgs = packages
                .Where(pkg => pkg.PackageName.EqualsIgnoreCase(DockerCePackageName) || pkg.PackageName.EqualsIgnoreCase(DockerEePackageName))
                .ToList();
            if (dockerPkgs.Any()) {
                var pkg = dockerPkgs.First();
                var files = pkg.GetPackageFiles(fs).Where(f => f.Equals(dockerPath));
                if (files.Any()) {
                    var docker = new LocalDocker(Path.GetDirectoryName(dockerPath), dockerPath);
                    if (!fs.FileExists(docker.DockerCommandPath)) {
                        throw new FileNotFoundException(Resources.Error_NoDockerCommand.FormatInvariant(docker.DockerCommandPath));
                    }

                    return docker;
                }
            }

            throw new ArgumentException(Resources.Error_DockerNotFound.FormatInvariant(DockerCePackageName, DockerEePackageName));
        }
    }
}
