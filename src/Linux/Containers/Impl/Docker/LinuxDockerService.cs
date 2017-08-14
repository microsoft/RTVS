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


using static System.FormattableString;
namespace Microsoft.R.Containers.Docker {
    public class LinuxDockerService : LocalDockerService, IContainerService {
        const string DockerCePackageName = "docker-ce";
        const string DockerEePackageName = "docker-ee";
        const string DockerProcessName = "dockerd";

        private readonly IFileSystem _fs;
        private readonly IProcessServices _ps;


        public LinuxDockerService(IFileSystem fs, IProcessServices ps, IActionLogWriter logWriter = null) : base(GetLocalDocker(fs), ps, logWriter) {
            _fs = fs;
            _ps = ps;
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
            return (processes.Count() >= 1) ? processes.FirstOrDefault() : null;
        }

        public async Task<IContainer> CreateContainerAsync(ContainerCreateParameters createParams, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
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

        public async new Task DeleteContainerAsync(IContainer container, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            var result = await base.DeleteContainerAsync(container, ct);
            if (!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerDeleteFailed.FormatInvariant(container.Id, result));
            }
        }

        public async new Task StartContainerAsync(IContainer container, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            var result = await base.StartContainerAsync(container, ct);
            if (!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerStartFailed.FormatInvariant(container.Id, result));
            }
        }

        public async new Task StopContainerAsync(IContainer container, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            var result = await base.StopContainerAsync(container, ct);
            if (!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerStopFailed.FormatInvariant(container.Id, result));
            }
        }

        private static LocalDocker GetLocalDocker(IFileSystem fs) {
            const string dockerPath = "/usr/bin/docker";
            LocalDocker docker = new LocalDocker();
            var packages = InstalledPackageInfo.GetPackages(fs);
            var dockerPkgs = packages.Where(pkg => pkg.PackageName.EqualsIgnoreCase(DockerCePackageName) || pkg.PackageName.EqualsIgnoreCase(DockerEePackageName));
            if (dockerPkgs.Count() > 0) {
                var pkg = dockerPkgs.First();
                var files = pkg.GetPackageFiles(fs).Where(f => f.Equals(dockerPath));
                if (files.Count() > 0) {
                    docker = new LocalDocker(Path.GetDirectoryName(dockerPath), pkg.Version, dockerPath);
                }
            } else {
                throw new ArgumentException(Resources.Error_DockerNotFound.FormatInvariant(DockerCePackageName, DockerEePackageName));
            }

            if (!fs.FileExists(docker.DockerCommandPath)) {
                throw new FileNotFoundException(Resources.Error_NoDockerCommand.FormatInvariant(docker.DockerCommandPath));
            }

            return docker;
        }
    }
}
