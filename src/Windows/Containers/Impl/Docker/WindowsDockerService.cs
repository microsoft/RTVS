// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Win32;
using static System.FormattableString;

namespace Microsoft.R.Containers.Docker {
    public class WindowsDockerService : LocalDockerService, IContainerService {
        const string DockerServiceName = "Docker for Windows";
        private readonly IFileSystem _fs;
        private readonly IProcessServices _ps;
        private readonly IActionLogWriter _outputLogWriter;

        public WindowsDockerService(IFileSystem fs, IProcessServices ps, IRegistry registryService, IActionLogWriter logWriter = null) : base (GetLocalDocker(registryService, fs), ps, logWriter) {
            _fs = fs;
            _ps = ps;
            _outputLogWriter = logWriter;
        }

        public ContainerServiceStatus GetServiceStatus() {
            var proc = GetDockerProcess(DockerServiceName);
            if (proc.HasExited) {
                return new ContainerServiceStatus(false, Resources.Error_ServiceNotAvailable, ContainerServiceStatusType.Error);
            } else {
                return new ContainerServiceStatus(true, Resources.Info_ServiceAvailable, ContainerServiceStatusType.Information);
            }
        }

        internal static Process GetDockerProcess(string processName) {
            var processes = Process.GetProcessesByName(processName);
            return (processes.Length >= 1) ? processes.FirstOrDefault() : null;
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
                return new LocalDockerContainer(containerId);
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
            if(!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerStopFailed.FormatInvariant(container.Id, result));
            }
        }

        private static LocalDocker GetLocalDocker(IRegistry rs, IFileSystem fs) {
            const string dockerRegistryPath = @"SOFTWARE\Docker Inc.\Docker";
            const string dockerCommand = "docker.exe";

            LocalDocker docker;
            using (var hklm64 = rs.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var dockerRegKey = hklm64.OpenSubKey(dockerRegistryPath)) { 
                if(dockerRegKey == null) {
                    throw new ArgumentException(Resources.Error_DockerNotFound.FormatInvariant(dockerRegistryPath));
                }

                string[] subkeys = dockerRegKey.GetSubKeyNames();
                foreach (var subKey in subkeys) {
                    using (var key = dockerRegKey.OpenSubKey(subKey)) {
                        bool isInstallKey = key.GetValueNames().Where(v => v.EqualsIgnoreCase("BinPath") || v.EqualsIgnoreCase("Version")).Count() == 2;
                        if (isInstallKey) {
                            var binPath = ((string)key.GetValue("BinPath")).Trim('\"');
                            var version = (string)key.GetValue("Version");
                            var commandPath = Path.Combine(binPath, dockerCommand);
                            docker = new LocalDocker(binPath, version, commandPath);
                            break;
                        }
                    }
                }

                if (!fs.FileExists(docker.DockerCommandPath)) {
                    throw new FileNotFoundException(Resources.Error_NoDockerCommand.FormatInvariant(docker.DockerCommandPath));
                }
            }
            return docker;
        }
    }
}
