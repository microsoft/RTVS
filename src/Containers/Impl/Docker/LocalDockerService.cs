// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Newtonsoft.Json.Linq;
using static System.FormattableString;
using System.Linq;

namespace Microsoft.R.Containers.Docker {
    public abstract class LocalDockerService : IDockerService {
        private readonly IProcessServices _ps;
        private readonly LocalDocker _docker;
        private readonly IActionLogWriter _outputLogWriter;
        private readonly Regex _containerIdMatcher64 = new Regex("[0-9a-f]{64}", RegexOptions.IgnoreCase);
        private readonly Regex _containerIdMatcher12 = new Regex("[0-9a-f]{12}", RegexOptions.IgnoreCase);
        private int _defaultTimeout = 5000;

        public LocalDockerService(LocalDocker docker, IProcessServices ps, IActionLogWriter logWriter) {
            _docker = docker;
            _ps = ps;
            _outputLogWriter = logWriter;
        }

        public async Task<IEnumerable<string>> ListContainersAsync(bool getAll = true, CancellationToken ct = default(CancellationToken)) {
            TaskUtilities.AssertIsOnBackgroundThread();
            var command = "ps";
            var commandOptions = getAll ? "-a -q" : "-q";
            var output = await ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), _defaultTimeout, ct);
            var ids = output.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            return ids.Where(id => _containerIdMatcher12.IsMatch(id) || _containerIdMatcher64.IsMatch(id));
        }

        public async Task<IContainer> GetContainerAsync(string containerId, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            var ids = (await ListContainersAsync(true, ct)).Where(id=> containerId.StartsWithIgnoreCase(id));
            if (ids.Count() > 0) {
                JArray arr = await InspectContainerAsync(containerId, ct);
                if (arr.Count == 1) {
                    return new LocalDockerContainer((string)arr[0]["Id"]);
                }
            }
            return null;
        }

        public async Task<JArray> InspectContainerAsync(string containerId, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            var command = "container inspect";
            var commandOptions = containerId;
            var result = await ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), _defaultTimeout, ct);
            return JArray.Parse(result);
        }

        public Task<string> RepositoryLoginAsync(string username, string password, string server, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            string command = "login";
            string commandOptions = $"-u {username} -p {password} {server}";
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, ct);
        }

        public Task<string> RepositoryLoginAsync(RepositoryCredentials auth, CancellationToken ct) {
            return RepositoryLoginAsync(auth.Username, auth.Password, auth.RepositoryServer, ct);
        }

        public Task<string> RepositoryLogoutAsync(string server, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            string command = "logout";
            string commandOptions = server;
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, ct);
        }

        public Task<string> RepositoryLogoutAsync(RepositoryCredentials auth, CancellationToken ct) {
            return RepositoryLogoutAsync(auth.RepositoryServer, ct);
        }

        public Task<string> PullImageAsync(string fullImageName, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            string command = "pull";
            string commandOptions = fullImageName;
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, ct);
        }

        public async Task<string> CreateContainerAsync(string createOptions, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            string command = "create";
            string output = await ExecuteCommandAsync(Invariant($"{command} {createOptions}"), -1, ct);
            var matches = _containerIdMatcher64.Matches(output);
            if (matches.Count >= 1) {
                return matches[0].Value;
            }
            return string.Empty;
        }

        public Task<string> DeleteContainerAsync(IContainer container, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            string command = "rm";
            string commandOptions = $"{container.Id}";
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, ct);
        }

        public Task<string> StartContainerAsync(IContainer container, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            string command = "start";
            string commandOptions = $"{container.Id}";
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, ct);
        }

        public Task<string> StopContainerAsync(IContainer container, CancellationToken ct) {
            TaskUtilities.AssertIsOnBackgroundThread();
            string command = "stop";
            string commandOptions = $"{container.Id}";
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, ct);
        }

        private async Task<string> ExecuteCommandAsync(string arguments, int timeoutms, CancellationToken ct) {
            ProcessStartInfo psi = new ProcessStartInfo() {
                FileName = _docker.DockerCommandPath,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = _ps.Start(psi);
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
                _outputLogWriter?.Write(MessageCategory.Error, error);
                throw new ContainerException(error);
            }

            _outputLogWriter?.Write(MessageCategory.General, output);
            return output;
        }
    }
}
