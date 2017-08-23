// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceProcess;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using static System.FormattableString;


namespace Microsoft.R.Containers.Docker {
    public class WindowsDockerService : LocalDockerService, IContainerService {
        const string DockerServiceName = "Docker for Windows";
        private LocalDocker _docker;
        private readonly IServiceContainer _services;
        private readonly WindowsLocalDockerFinder _dockerFinder;

        public WindowsDockerService(IServiceContainer services) : base(services) {
            _services = services;
            _dockerFinder = new WindowsLocalDockerFinder(services);
        }

        public ContainerServiceStatus GetServiceStatus() => GetDockerProcess(DockerServiceName).HasExited 
            ? new ContainerServiceStatus(false, Resources.Error_ServiceNotAvailable, ContainerServiceStatusType.Error) 
            : new ContainerServiceStatus(true, Resources.Info_ServiceAvailable, ContainerServiceStatusType.Information);

        internal static Process GetDockerProcess(string processName) => Process.GetProcessesByName(processName).FirstOrDefault();

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
            if(!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerStopFailed.FormatInvariant(container.Id, result));
            }
        }

        protected override LocalDocker GetLocalDocker() {
            _docker = _docker ?? _dockerFinder.GetLocalDocker();
            _dockerFinder.CheckIfServiceIsRunning();

            return _docker;
        }

        

        
    }
}
