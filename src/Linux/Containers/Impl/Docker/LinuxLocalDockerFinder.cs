// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.R.Platform.OS.Linux;

namespace Microsoft.R.Containers.Docker {
    public class LinuxLocalDockerFinder {
        public const string DockerCePackageName = "docker-ce";
        public const string DockerEePackageName = "docker-ee";
        public const string DockerProcessName = "dockerd";
        private readonly IFileSystem _fs;
        private readonly IProcessServices _ps;

        public LinuxLocalDockerFinder(IServiceContainer services) {
            _fs = services.FileSystem();
            _ps = services.Process();
        }

        public void CheckIfServiceIsRunning() {
            const string dockerd = "dockerd";
            const string dockerContainerd = "docker-containerd";
            if (!_ps.IsProcessRunning(dockerd)) {
                throw new ContainerServiceNotRunningException(Resources.Error_DockerServiceNotRunning.FormatInvariant(dockerd));
            }

            // NOTE: the service is docker-containerd however ProcessName for the service is docker-containe.
            if (!_ps.IsProcessRunning("docker-containe")) {
                throw new ContainerServiceNotRunningException(Resources.Error_DockerServiceNotRunning.FormatInvariant(dockerContainerd));
            }
        }

        public LocalDocker GetLocalDocker() {
            const string dockerPath = "/usr/bin/docker";
            var packages = InstalledPackageInfo.GetPackages(_fs);
            var dockerPkgs = packages
                .Where(pkg => pkg.PackageName.EqualsIgnoreCase(DockerCePackageName) || pkg.PackageName.EqualsIgnoreCase(DockerEePackageName))
                .ToList();
            if (dockerPkgs.Any()) {
                var pkg = dockerPkgs.First();
                var files = pkg.GetPackageFiles(_fs).Where(f => f.Equals(dockerPath));
                if (files.Any()) {
                    var docker = new LocalDocker(Path.GetDirectoryName(dockerPath), dockerPath);
                    if (!_fs.FileExists(docker.DockerCommandPath)) {
                        throw new ContainerServiceNotInstalledException(Resources.Error_NoDockerCommand.FormatInvariant(docker.DockerCommandPath));
                    }

                    return docker;
                }
            }

            throw new ContainerServiceNotInstalledException(Resources.Error_DockerNotFound.FormatInvariant(DockerCePackageName, DockerEePackageName));
        }
    }
}
