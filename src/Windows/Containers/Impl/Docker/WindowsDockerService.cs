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
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Win32;
using static System.FormattableString;


namespace Microsoft.R.Containers.Docker {
    public class WindowsDockerService : LocalDockerService, IContainerService {
        const string DockerServiceName = "Docker for Windows";
        const string dockerCommand = "docker.exe";
        const string dockerRegistryPath = @"SOFTWARE\Docker Inc.\Docker";
        const string dockerRegistryPath2 = @"SYSTEM\CurrentControlSet\Services\com.docker.service";
        private LocalDocker _docker;
        private readonly IServiceContainer _services;

        public WindowsDockerService(IServiceContainer services) : base(services) {
            _services = services;
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
            _docker = _docker ?? GetLocalDocker(_services);
            CheckIfServiceIsRunning();

            return _docker;
        }

        private static void CheckIfServiceIsRunning() {
            const string serviceName = "com.docker.service";
            ServiceController sc = new ServiceController(serviceName);
            if (sc.Status != ServiceControllerStatus.Running) {
                throw new ContainerServiceNotRunningException(Resources.Error_DockerServiceNotRunning.FormatInvariant(sc.Status.ToString()));
            }

            if (!Process.GetProcessesByName("Docker for windows").Any()) {
                throw new ContainerServiceNotRunningException(Resources.Error_DockerForWindowsNotRunning);
            }
        }

        private static LocalDocker GetLocalDocker(IServiceContainer services) {
            var rs = services.GetService<IRegistry>();
            var fs = services.FileSystem();
            LocalDocker docker = null;
            using (var hklm64 = rs.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                if(!TryGetDockerFromRegistryInstall(fs, hklm64, out docker) &&
                   !TryGetDockerFromServiceInstall(fs, hklm64, out docker) &&
                   !TryGetDockerFromProgramFiles(fs,out docker)) {
                    throw new ContainerServiceNotInstalledException(Resources.Error_DockerNotFound.FormatInvariant(dockerRegistryPath));
                }
            }

            if (!fs.FileExists(docker.DockerCommandPath)) {
                throw new ContainerServiceNotInstalledException(Resources.Error_NoDockerCommand.FormatInvariant(docker.DockerCommandPath));
            }

            return docker;
        }

        private static bool TryGetDockerFromProgramFiles(IFileSystem fs, out LocalDocker docker) {
            string[] envVars = { "ProgramFiles", "ProgramFiles(x86)", "ProgramW6432" };
            foreach (var envVar in envVars) {
                var progFiles = Environment.GetEnvironmentVariable(envVar);
                if (!string.IsNullOrWhiteSpace(progFiles)) {
                    var basePath = Path.Combine(progFiles, "Docker", "Docker");
                    var binPath = Path.Combine(basePath, "resources", "bin");
                    var commandPath = Path.Combine(binPath, dockerCommand);
                    if (fs.FileExists(commandPath)) {
                        docker = new LocalDocker(binPath, commandPath);
                        return true;
                    }
                }
            }

            docker = null;
            return false;
        }

        private static bool TryGetDockerFromRegistryInstall(IFileSystem fs, IRegistryKey hklm, out LocalDocker docker) {
            using (var dockerRegKey = hklm.OpenSubKey(dockerRegistryPath)) {
                if (dockerRegKey != null) {
                    string[] subkeys = dockerRegKey.GetSubKeyNames();
                    foreach (var subKey in subkeys) {
                        using (var key = dockerRegKey.OpenSubKey(subKey)) {
                            var isInstallKey = key.GetValueNames().Count(v => v.Equals("BinPath") || v.Equals("Version")) == 2;
                            if (isInstallKey) {
                                var binPath = ((string)key.GetValue("BinPath")).Trim('\"');
                                var commandPath = Path.Combine(binPath, dockerCommand);
                                if (fs.FileExists(commandPath)) {
                                    docker = new LocalDocker(binPath, commandPath);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            docker = null;
            return false;
        }

        private static bool TryGetDockerFromServiceInstall(IFileSystem fs, IRegistryKey hklm, out LocalDocker docker) {
            using (var dockerRegKey = hklm.OpenSubKey(dockerRegistryPath)) {
                if (dockerRegKey != null) {
                    var valueNames = dockerRegKey.GetValueNames();
                    if (valueNames.Contains("ImagePath")) {
                        var comPath = ((string)dockerRegKey.GetValue("ImagePath")).Trim('\"');
                        var basePath = Path.GetDirectoryName(comPath);
                        var binPath = Path.Combine(basePath, "resources", "bin");
                        var commandPath = Path.Combine(binPath, dockerCommand);
                        if (fs.FileExists(commandPath)) {
                            docker = new LocalDocker(binPath, commandPath);
                            return true;
                        }
                    }
                }
            }

            docker = null;
            return false;
        }
    }
}
