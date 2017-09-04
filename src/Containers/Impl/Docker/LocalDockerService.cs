// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Containers.Docker {
    public abstract class LocalDockerService : IDockerService {
        private readonly IProcessServices _ps;
        private readonly IActionLogWriter _outputLogWriter;
        private readonly Regex _containerIdMatcher64 = new Regex("[0-9a-f]{64}", RegexOptions.IgnoreCase);
        private readonly Regex _containerIdMatcher12 = new Regex("[0-9a-f]{12}", RegexOptions.IgnoreCase);
        private readonly int _defaultTimeout = 500;

        protected LocalDockerService(IServiceContainer services) {
            _ps = services.Process();
            _outputLogWriter = services.GetService<IActionLogWriter>();
            // TODO: No instance of IActionLogWriter is exported in default IServiceContainer. Need scope support.
        }

        public Task<string> BuildImageAsync(string buildOptions, CancellationToken ct) {
            var command = "build";
            return ExecuteCommandAsync(Invariant($"{command} {buildOptions}"), -1, true, ct);
        }

        public async Task<IEnumerable<IContainer>> ListContainersAsync(bool getAll = true, CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            var command = "ps";
            var commandOptions = getAll ? "-a -q" : "-q";
            var output = await ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), _defaultTimeout, true, ct);
            var lines = output.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var ids = lines.Where(line => _containerIdMatcher12.IsMatch(line) || _containerIdMatcher64.IsMatch(line));
            var arr = await InspectContainerAsync(ids, ct);
            return arr.Select(c => new LocalDockerContainer(c));
        }

        public async Task<IContainer> GetContainerAsync(string containerId, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            var ids = (await ListContainersAsync(true, ct)).Where(container => containerId.StartsWithIgnoreCase(container.Id));
            if (ids.Any()) {
                var arr = await InspectContainerAsync(new string[] { containerId }, ct);
                if (arr.Count == 1) {
                    return new LocalDockerContainer(arr[0]);
                }
            }

            return null;
        }

        public async Task<JArray> InspectContainerAsync(IEnumerable<string> containerIds, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            var ids = containerIds.AsList();
            if (ids.Any()) {
                var command = "container inspect";
                var commandOptions = string.Join(" ", ids);
                var result = await ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), _defaultTimeout, false, ct);
                return JArray.Parse(result);
            }

            return new JArray();
        }

        public Task<string> RepositoryLoginAsync(string username, string password, string server, CancellationToken ct) {
            var command = "login";
            var commandOptions = $"-u {username} -p {password} {server}";
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, true, ct);
        }

        public Task<string> RepositoryLoginAsync(RepositoryCredentials auth, CancellationToken ct) 
            => RepositoryLoginAsync(auth.Username, auth.Password, auth.RepositoryServer, ct);

        public Task<string> RepositoryLogoutAsync(string server, CancellationToken ct) {
            var command = "logout";
            var commandOptions = server;
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, true, ct);
        }

        public Task<string> RepositoryLogoutAsync(RepositoryCredentials auth, CancellationToken ct) 
            => RepositoryLogoutAsync(auth.RepositoryServer, ct);

        public Task<string> PullImageAsync(string fullImageName, CancellationToken ct) {
            var command = "pull";
            var commandOptions = fullImageName;
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, true, ct);
        }

        public async Task<string> CreateContainerAsync(string createOptions, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            var command = "create";
            var output = await ExecuteCommandAsync(Invariant($"{command} {createOptions}"), -1, true, ct);
            var matches = _containerIdMatcher64.Matches(output);

            return matches.Count >= 1 ? matches[0].Value : string.Empty;
        }

        public Task<string> DeleteContainerAsync(IContainer container, CancellationToken ct) {
            var command = "rm";
            var commandOptions = $"{container.Id}";
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, true, ct);
        }

        public Task<string> StartContainerAsync(IContainer container, CancellationToken ct) {
            var command = "start";
            var commandOptions = $"{container.Id}";
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, true, ct);
        }

        public Task<string> StopContainerAsync(IContainer container, CancellationToken ct) {
            var command = "stop";
            var commandOptions = $"{container.Id}";
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), -1, true, ct);
        }

        protected abstract LocalDocker GetLocalDocker();

        private async Task<string> ExecuteCommandAsync(string arguments, int timeoutms, bool failOnTimeout = true, CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            var docker = GetLocalDocker();
            var psi = new ProcessStartInfo {
                CreateNoWindow = true,
                FileName = docker.DockerCommandPath,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = _ps.Start(psi);
            try {
                await process.WaitForExitAsync(timeoutms, ct);
            } catch(OperationCanceledException) when (!failOnTimeout && !ct.IsCancellationRequested){
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            if (!string.IsNullOrEmpty(error)) {
                _outputLogWriter?.Write(MessageCategory.Error, error);
                throw new ContainerException(error);
            }

            _outputLogWriter?.Write(MessageCategory.General, output);
            return output;
        }
    }
}
