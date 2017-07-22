// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Win32;
using static System.FormattableString;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Containers.Docker {
    public class WindowsDockerService : IContainerService {
        const string DockerServiceName = "Docker for Windows";
        private readonly LocalDocker _docker;
        private readonly IRegistry _registryService;
        private readonly IFileSystem _fs;
        private readonly IProcessServices _ps;
        private readonly Regex _containerIdMatcher64 = new Regex("[0-9a-f]{64}", RegexOptions.IgnoreCase);
        private readonly Regex _containerIdMatcher12 = new Regex("[0-9a-f]{12}", RegexOptions.IgnoreCase);

        public WindowsDockerService(IFileSystem fs, IProcessServices ps, IRegistry registryService) {
            _fs = fs;
            _ps = ps;
            _registryService = registryService;
            _docker = GetLocalDocker();
        }

        public ContainerServiceStatus GetServiceStatus() {
            ContainerServiceStatus status = new ContainerServiceStatus();
            var proc = GetDockerProcess(DockerServiceName);
            if (proc.HasExited) {
                status.IsServiceAvailable = false;
                status.StatusType = ContainerServiceStatusType.Error;
                status.StatusMessage = Resources.Error_ServiceNotAvailable;
            } else {
                status.IsServiceAvailable = true;
                status.StatusType = ContainerServiceStatusType.Information;
                status.StatusMessage = Resources.Info_ServiceAvailable;
            }
            return status;
        }

        internal static Process GetDockerProcess(string processName) {
            var processes = Process.GetProcessesByName(processName);
            return (processes.Length >= 1) ? processes.First() : null;
        }

        public async Task<IContainer> GetContainerAsync(string containerId, CancellationToken ct) {
            string output = await _docker.ListContainersAsync(_ps, true, ct);
            var ids = output.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string id in ids) {
                if (containerId.StartsWithIgnoreCase(id)) {
                    JArray arr = await _docker.InspectContainerAsync(_ps, containerId, ct);
                    if (arr.Count == 1) {
                        return new LocalDockerContainer() { Id = (string)arr[0]["Id"] };
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<string>> ListContainersAsync(bool allContainers, CancellationToken ct) {
            string output = await _docker.ListContainersAsync(_ps, allContainers, ct);
            var ids = output.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> containerIds = new List<string>();
            foreach(string id in ids) {
                if (_containerIdMatcher12.IsMatch(id)) {
                    containerIds.Add(id);
                }
            }
            return containerIds;
        }

        public async Task<IContainer> CreateContainerAsync(ContainerCreateParameters createParams, CancellationToken ct) {
            if(createParams.ImageSourceAuth != null) {
                await _docker.RepositoryLoginAsync(_ps, createParams.ImageSourceAuth, ct);
            }
            try {
                await _docker.PullImageAsync(_ps, $"{createParams.Image}:{createParams.Tag}", ct);

                string createOptions = $"{createParams.StartOptions} {createParams.Image}:{createParams.Tag} {createParams.Command}";
                var result = await _docker.CreateContainerAsync(_ps, createOptions, ct);
                string containerId = result.Substring(0, 64);
                if (!_containerIdMatcher64.IsMatch(containerId)) {
                    throw new ContainerException(Resources.Error_ContainerIdInvalid.FormatInvariant(containerId));
                }
                return new LocalDockerContainer() { Id = containerId };
            } catch (Exception) {
                throw;
            } finally {
                if (createParams.ImageSourceAuth != null) {
                    await _docker.RepositoryLogoutAsync(_ps, createParams.ImageSourceAuth, ct);
                }
            }
        }

        public async Task DeleteContainerAsync(IContainer container, CancellationToken ct) {
            var result = await _docker.DeleteContainerAsync(_ps, container, ct);
            if (!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerDeleteFailed.FormatInvariant(container.Id, result));
            }
        }

        public async Task StartContainerAsync(IContainer container, CancellationToken ct) {
            var result = await _docker.StartContainerAsync(_ps, container, ct);
            if (!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerStartFailed.FormatInvariant(container.Id, result));
            }
        }

        public async Task StopContainerAsync(IContainer container, CancellationToken ct) {
            var result = await _docker.StopContainerAsync(_ps, container, ct);
            if(!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerStopFailed.FormatInvariant(container.Id, result));
            }
        }

        private LocalDocker GetLocalDocker() {
            const string dockerRegistryPath = @"SOFTWARE\Docker Inc.\Docker";
            const string dockerCommand = "docker.exe";

            LocalDocker docker = new LocalDocker();
            using (var hklm64 = _registryService.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var dockerRegKey = hklm64.OpenSubKey(dockerRegistryPath)) { 
                if(dockerRegKey == null) {
                    throw new ArgumentException(Resources.Error_DockerNotFound.FormatInvariant(dockerRegistryPath));
                }

                string[] subkeys = dockerRegKey.GetSubKeyNames();
                foreach (string subKey in subkeys) {
                    using (var key = dockerRegKey.OpenSubKey(subKey)) {
                        bool isInstallKey = key.GetValueNames().Where(v => v.EqualsIgnoreCase("BinPath") || v.EqualsIgnoreCase("AppPath") || v.EqualsIgnoreCase("Version")).Count() == 3;
                        if (isInstallKey) {
                            docker.BinPath = ((string)key.GetValue("BinPath")).Trim('\"');
                            docker.Docker = Path.Combine(docker.BinPath, dockerCommand);
                            docker.InstallPath = ((string)key.GetValue("AppPath")).Trim('\"');
                            docker.Version = (string)key.GetValue("Version");
                            break;
                        }
                    }
                }

                if (!_fs.FileExists(docker.Docker)) {
                    throw new FileNotFoundException(Resources.Error_NoDockerCommand.FormatInvariant(docker.Docker));
                }
            }
            return docker;
        }
    }
}
