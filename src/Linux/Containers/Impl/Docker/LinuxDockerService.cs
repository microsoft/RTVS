// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using static System.FormattableString;

namespace Microsoft.R.Containers.Docker {
    public class LinuxDockerService : LocalDockerService, IContainerService {
        private LocalDocker _docker;
        private readonly IServiceContainer _services;
        private readonly LinuxLocalDockerFinder _dockerFinder;

        public LinuxDockerService(IServiceContainer services) : base(services) {
            _services = services;
            _dockerFinder = new LinuxLocalDockerFinder(services);
        }

        public ContainerServiceStatus GetServiceStatus() => GetDockerProcess().HasExited
            ? new ContainerServiceStatus(false, Resources.Error_ServiceNotAvailable, ContainerServiceStatusType.Error)
            : new ContainerServiceStatus(true, Resources.Info_ServiceAvailable, ContainerServiceStatusType.Information);

        internal static Process GetDockerProcess() => Process.GetProcessesByName(LinuxLocalDockerFinder.DockerProcessName).FirstOrDefault();

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

        async Task IContainerService.DeleteContainerAsync(string containerId, CancellationToken ct) {
            var result = await DeleteContainerAsync(containerId, ct);
            if (!result.StartsWithIgnoreCase(containerId)) {
                throw new ContainerException(Resources.Error_ContainerDeleteFailed.FormatInvariant(containerId, result));
            }
        }

        async Task IContainerService.StartContainerAsync(string containerId, CancellationToken ct) {
            var result = await StartContainerAsync(containerId, ct);
            if (!result.StartsWithIgnoreCase(containerId)) {
                throw new ContainerException(Resources.Error_ContainerStartFailed.FormatInvariant(containerId, result));
            }
        }

        async Task IContainerService.StopContainerAsync(string containerId, CancellationToken ct) {
            var result = await StopContainerAsync(containerId, ct);
            if (!result.StartsWithIgnoreCase(containerId)) {
                throw new ContainerException(Resources.Error_ContainerStopFailed.FormatInvariant(containerId, result));
            }
        }

        protected override LocalDocker GetLocalDocker() {
            _docker = _docker ?? _dockerFinder.GetLocalDocker();
            _dockerFinder.CheckIfServiceIsRunning();

            return _docker;
        }

        
    }
}
