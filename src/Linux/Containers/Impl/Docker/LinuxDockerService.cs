// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Microsoft.R.Containers.Docker {
    public class LinuxDockerService : IContainerService {
        const string DockerCePackageName = "docker-ce";
        const string DockerEePackageName = "docker-ee";
        const string DockerProcessName = "dockerd";

        private readonly LocalDocker _docker;
        private readonly IFileSystem _fs;
        private readonly IProcessServices _ps;
        private readonly Regex _containerIdMatcher64 = new Regex("[0-9a-f]{64}", RegexOptions.IgnoreCase);
        private readonly Regex _containerIdMatcher12 = new Regex("[0-9a-f]{12}", RegexOptions.IgnoreCase);

        public LinuxDockerService(IFileSystem fs, IProcessServices ps) {
            _fs = fs;
            _ps = ps;
            _docker = GetLocalDocker();
        }

        public ContainerServiceStatus GetServiceStatus() {
            ContainerServiceStatus status = new ContainerServiceStatus();
            var proc = GetDockerProcess();
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

        internal static Process GetDockerProcess() {
            var processes = Process.GetProcessesByName(DockerProcessName);
            return (processes.Count() >= 1) ? processes.First() : null;
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

        public async Task<IEnumerable<string>> ListContainersAsync(CancellationToken ct) {
            string output = await _docker.ListContainersAsync(_ps, true, ct);
            var ids = output.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> containerIds = new List<string>();
            foreach (string id in ids) {
                if (_containerIdMatcher12.IsMatch(id)) {
                    containerIds.Add(id);
                }
            }
            return containerIds;
        }

        public async Task<IContainer> CreateContainerAsync(ContainerCreateParameters createParams, CancellationToken ct) {
            if (createParams.ImageSourceAuth != null) {
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
            if (!result.StartsWithIgnoreCase(container.Id)) {
                throw new ContainerException(Resources.Error_ContainerStopFailed.FormatInvariant(container.Id, result));
            }
        }

        private LocalDocker GetLocalDocker() {
            const string dockerPath = "/usr/bin/docker";
            LocalDocker docker = new LocalDocker();
            var packages = InstalledPackageInfo.GetPackages(_fs);
            var dockerPkgs = packages.Where(pkg => pkg.PackageName.EqualsIgnoreCase(DockerCePackageName) || pkg.PackageName.EqualsIgnoreCase(DockerEePackageName));
            if (dockerPkgs.Count() > 0) {
                var pkg = dockerPkgs.First();
                var files = pkg.GetPackageFiles(_fs).Where(f => f.Equals(dockerPath));
                if (files.Count() > 0) {
                    docker.BinPath = Path.GetDirectoryName(dockerPath);
                    docker.Docker = dockerPath;
                    docker.Version = pkg.Version;
                    docker.Version = Path.Combine("/usr/share", pkg.PackageName);
                }
            } else {
                throw new ArgumentException(Resources.Error_DockerNotFound.FormatInvariant(DockerCePackageName, DockerEePackageName));
            }

            if (!_fs.FileExists(docker.Docker)) {
                throw new FileNotFoundException(Resources.Error_NoDockerCommand.FormatInvariant(docker.Docker));
            }

            return docker;
        }
    }
}
