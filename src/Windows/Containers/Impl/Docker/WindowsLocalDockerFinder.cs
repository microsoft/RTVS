// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.R.Platform.OS;
using Microsoft.Win32;

namespace Microsoft.R.Containers.Docker {
    public class WindowsLocalDockerFinder {
        public const string DockerCommand = "docker.exe";
        public const string DockerRegistryPath = @"SOFTWARE\Docker Inc.\Docker";
        public const string DockerRegistryPath2 = @"SYSTEM\CurrentControlSet\Services\com.docker.service";
        private readonly IFileSystem _fs;
        private readonly IRegistry _rs;
        private readonly IProcessServices _ps;

        public WindowsLocalDockerFinder(IServiceContainer services) {
            _fs = services.FileSystem();
            _rs = services.GetService<IRegistry>();
            _ps = services.Process();
        }

        public LocalDocker GetLocalDocker() {
            LocalDocker docker = null;
            using (var hklm64 = _rs.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                if (!TryGetDockerFromRegistryInstall(_fs, hklm64, out docker) &&
                   !TryGetDockerFromServiceInstall(_fs, hklm64, out docker) &&
                   !TryGetDockerFromProgramFiles(_fs, out docker)) {
                    throw new ContainerServiceNotInstalledException(Resources.Error_DockerNotFound.FormatInvariant(DockerRegistryPath));
                }
            }

            if (!_fs.FileExists(docker.DockerCommandPath)) {
                throw new ContainerServiceNotInstalledException(Resources.Error_NoDockerCommand.FormatInvariant(docker.DockerCommandPath));
            }

            return docker;
        }

        public void CheckIfServiceIsRunning() {
            const string serviceName = "com.docker.service";
            using (var sc = new ServiceController(serviceName)) {
                if (sc.Status != ServiceControllerStatus.Running) {
                    throw new ContainerServiceNotRunningException(Resources.Error_DockerServiceNotRunning.FormatInvariant(sc.Status.ToString()));
                }

                if (!_ps.IsProcessRunning("Docker for windows")) {
                    throw new ContainerServiceNotRunningException(Resources.Error_DockerForWindowsNotRunning);
                }
            }
        }

        private static bool TryGetDockerFromProgramFiles(IFileSystem fs, out LocalDocker docker) {
            string[] envVars = { "ProgramFiles", "ProgramFiles(x86)", "ProgramW6432" };
            foreach (var envVar in envVars) {
                var progFiles = Environment.GetEnvironmentVariable(envVar);
                if (!string.IsNullOrWhiteSpace(progFiles)) {
                    var basePath = Path.Combine(progFiles, "Docker", "Docker");
                    var binPath = Path.Combine(basePath, "resources", "bin");
                    var commandPath = Path.Combine(binPath, DockerCommand);
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
            using (var dockerRegKey = hklm.OpenSubKey(DockerRegistryPath)) {
                if (dockerRegKey != null) {
                    string[] subkeys = dockerRegKey.GetSubKeyNames();
                    foreach (var subKey in subkeys) {
                        using (var key = dockerRegKey.OpenSubKey(subKey)) {
                            var isInstallKey = key.GetValueNames().Count(v => v.Equals("BinPath") || v.Equals("Version")) == 2;
                            if (isInstallKey) {
                                var binPath = ((string)key.GetValue("BinPath")).Trim('\"');
                                var commandPath = Path.Combine(binPath, DockerCommand);
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
            using (var dockerRegKey = hklm.OpenSubKey(DockerRegistryPath2)) {
                if (dockerRegKey != null) {
                    var valueNames = dockerRegKey.GetValueNames();
                    if (valueNames.Contains("ImagePath")) {
                        var comPath = ((string)dockerRegKey.GetValue("ImagePath")).Trim('\"');
                        var basePath = Path.GetDirectoryName(comPath);
                        var binPath = Path.Combine(basePath, "resources", "bin");
                        var commandPath = Path.Combine(binPath, DockerCommand);
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
