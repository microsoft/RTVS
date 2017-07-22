// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Common.Core.OS;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Containers.Docker {
    public static class LocalDockerExtensions {
        public static async Task<string> ListContainersAsync(this LocalDocker dockerInfo, IProcessServices ps, bool getAll=true, CancellationToken ct = default(CancellationToken)) {
            string command = "ps";
            string commandOptions = getAll ? "-a -q" : "-q";
            return await ExecuteCommandAsync(ps, dockerInfo.Docker, $"{command} {commandOptions}", 5000, ct);
        }

        public static async Task<JArray> InspectContainerAsync(this LocalDocker dockerInfo, IProcessServices ps, string containerId, CancellationToken ct) {
            string command = "container inspect";
            string commandOptions = containerId;
            string result = await ExecuteCommandAsync(ps, dockerInfo.Docker, $"{command} {commandOptions}", 5000, ct);
            return JArray.Parse(result);
        }
        public static async Task<string> RepositoryLoginAsync(this LocalDocker dockerInfo, IProcessServices ps, string username, string password, string server, CancellationToken ct) {
            string command = "login";
            string commandOptions = $"-u {username} -p {password} {server}";
            return await ExecuteCommandAsync(ps, dockerInfo.Docker, $"{command} {commandOptions}", -1, ct);
        }

        public static Task<string> RepositoryLoginAsync(this LocalDocker dockerInfo, IProcessServices ps, RepositoryCredentials auth, CancellationToken ct) {
            return dockerInfo.RepositoryLoginAsync(ps, auth.Username, auth.Password, auth.RepositoryServer, ct);
        }

        public static Task<string> RepositoryLogoutAsync(this LocalDocker dockerInfo, IProcessServices ps, RepositoryCredentials auth, CancellationToken ct) {
            return dockerInfo.RepositoryLogoutAsync(ps, auth.RepositoryServer, ct);
        }

        public static async Task<string> RepositoryLogoutAsync(this LocalDocker dockerInfo, IProcessServices ps, string server, CancellationToken ct) {
            string command = "logout";
            string commandOptions = server;
            return await ExecuteCommandAsync(ps, dockerInfo.Docker, $"{command} {commandOptions}", -1, ct);
        }

        public static async Task<string> PullImageAsync(this LocalDocker dockerInfo, IProcessServices ps, string fullImageName, CancellationToken ct) {
            string command = "pull";
            string commandOptions = fullImageName;
            return await ExecuteCommandAsync(ps, dockerInfo.Docker, $"{command} {commandOptions}", -1, ct);
        }

        public static async Task<string> CreateContainerAsync(this LocalDocker dockerInfo, IProcessServices ps, string createOptions, CancellationToken ct) {
            string command = "create";
            return await ExecuteCommandAsync(ps, dockerInfo.Docker, $"{command} {createOptions}", -1, ct);
        }

        public static async Task<string> DeleteContainerAsync(this LocalDocker dockerInfo, IProcessServices ps, IContainer container, CancellationToken ct) {
            string command = "rm";
            string commandOptions = $"{container.Id}";
            return await ExecuteCommandAsync(ps, dockerInfo.Docker, $"{command} {commandOptions}", -1, ct);
        }

        public static async Task<string> StartContainerAsync(this LocalDocker dockerInfo, IProcessServices ps, IContainer container, CancellationToken ct) {
            string command = "start";
            string commandOptions = $"{container.Id}";
            return await ExecuteCommandAsync(ps, dockerInfo.Docker, $"{command} {commandOptions}", -1, ct);
        }

        public static async Task<string> StopContainerAsync(this LocalDocker dockerInfo, IProcessServices ps, IContainer container, CancellationToken ct) {
            string command = "stop";
            string commandOptions = $"{container.Id}";
            return await ExecuteCommandAsync(ps, dockerInfo.Docker, $"{command} {commandOptions}", -1, ct);
        }

        private static async Task<string> ExecuteCommandAsync(IProcessServices ps, string commandFile, string arguments, int timeoutms = 5000, CancellationToken ct = default(CancellationToken)) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = commandFile;
            psi.Arguments = arguments;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;

            var process = ps.Start(psi);
            await Task.Run(() => {
                if (timeoutms < 0) {
                    process.WaitForExit();
                } else {
                    process.WaitForExit(timeoutms);
                }
            }, ct);
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            if (!string.IsNullOrEmpty(error)) {
                throw new ContainerException(error);
            }
            return output;
        }
    }
}
