// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Debugger;
using Microsoft.R.Debugger.PortSupplier;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Extensions;
using Microsoft.VisualStudio.ProjectSystem;
using static System.FormattableString;
using System.Threading;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Debuggers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.DebuggerProviders;
using Microsoft.VisualStudio.ProjectSystem.VS.Debuggers;
#else
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    // ExportDebugger must match rule name in ..\Rules\Debugger.xaml.
    [ExportDebugger("RDebugger")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal class RDebugLaunchProvider : DebugLaunchProviderBase {
        private readonly ProjectProperties _properties;
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly IProjectSystemServices _pss;

        [ImportingConstructor]
        public RDebugLaunchProvider(ConfiguredProject configuredProject, IRInteractiveWorkflowProvider interactiveWorkflowProvider, IProjectSystemServices pss)
            : base(configuredProject) {
            _properties = configuredProject.Services.ExportProvider.GetExportedValue<ProjectProperties>();
            _interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();
            _pss = pss;
        }

        internal IFileSystem FileSystem { get; set; } = new FileSystem();

        private IRSession Session => _interactiveWorkflow.RSession;
        private IRCallbacks Callbacks => (IRCallbacks)_interactiveWorkflow.RSession;

        public override Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions) {
            return Task.FromResult(true);
        }

        public override Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions) {
            var targets = new List<IDebugLaunchSettings>();

            if (Session.IsHostRunning) {
                uint pid = RDebugPortSupplier.GetProcessId(Session.Id);

                var target = new DebugLaunchSettings(launchOptions) {
                    LaunchOperation = DebugLaunchOperation.AlreadyRunning,
                    PortSupplierGuid = DebuggerGuids.PortSupplier,
                    PortName = RDebugPortSupplier.PortName,
                    LaunchDebugEngineGuid = DebuggerGuids.DebugEngine,
                    ProcessId = (int)pid,
                    Executable = RDebugPortSupplier.GetExecutableForAttach(pid),
                };

                targets.Add(target);
            }

            return Task.FromResult((IReadOnlyList<IDebugLaunchSettings>)targets);
        }

        public override async Task LaunchAsync(DebugLaunchOptions launchOptions) {
            // Reset first, before attaching debugger via LaunchAsync (since it'll detach on reset).
            if (await _properties.GetResetReplOnRunAsync()) {
                await _interactiveWorkflow.Operations.ResetAsync();
            }

            // Base implementation will try to launch or attach via the debugger, but there's nothing to launch
            // in case of Ctrl+F5 - we only want to source the file. So only invoke base if we intend to debug.
            if (!launchOptions.HasFlag(DebugLaunchOptions.NoDebug)) {
                await base.LaunchAsync(launchOptions);
            }

            _interactiveWorkflow.ActiveWindow?.Container.Show(false);

            bool transferFiles = await _properties.GetTransferProjectOnRunAsync();
            string remotePath = await _properties.GetRemoteProjectPathAsync();
            string filterString = await _properties.GetFileFilterAsync();

            var activeProject = _pss.GetActiveProject();
            if (transferFiles && Session.IsRemoteSession && activeProject != null) {
                await SendProjectAsync(activeProject, remotePath, filterString);
            }

            // user must set the path for local or remote cases
            var startupFile = await GetStartupFileAsync(transferFiles, activeProject);

            if (string.IsNullOrWhiteSpace(startupFile)) {
                _interactiveWorkflow.ActiveWindow?.InteractiveWindow.WriteErrorLine(Resources.Launch_NoStartupFile);
                return;
            }
            await SourceFileAsync(transferFiles, startupFile, Invariant($"{Resources.Launch_StartupFileDoesNotExist} {startupFile}"));

            var settingsFile = await _properties.GetSettingsFileAsync();
            if (!string.IsNullOrWhiteSpace(settingsFile)) {
                if (activeProject != null) {
                    var dirPath = Path.GetDirectoryName(activeProject.FullName);
                    settingsFile = settingsFile.MakeAbsolutePathFromRRelative(dirPath);
                    if (FileSystem.FileExists(settingsFile)) {
                        if (Session.IsRemoteSession) {
                            var remoteSettingsPath = GetRemoteSettingsFile(settingsFile, dirPath, remotePath);
                            await SourceFileAsync(transferFiles, remoteSettingsPath, Invariant($"{Resources.Launch_SettingsFileDoesNotExist} {settingsFile}"));
                        } else {
                            await SourceFileAsync(transferFiles, settingsFile, Invariant($"{Resources.Launch_SettingsFileDoesNotExist} {settingsFile}"));
                        }
                    }
                }
            }
        }

        private async Task SourceFileAsync(bool transferFiles, string file, string errorMessage) {
            bool fExists = false;
            if (transferFiles && Session.IsRemoteSession) {
                fExists = await Session.EvaluateAsync<bool>(Invariant($"file.exists({file.ToRPath().ToRStringLiteral()})"), REvaluationKind.Normal);
            } else {
                fExists = FileSystem.FileExists(file);
            }

            if (!fExists) {
                _interactiveWorkflow.ActiveWindow?.InteractiveWindow.WriteErrorLine(errorMessage);
                return;
            }

            await Callbacks.WriteConsoleEx($"Sourcing: {file}\n", OutputType.Output, CancellationToken.None);
            await _interactiveWorkflow.Operations.SourceFileAsync(file, echo: false).SilenceException<Exception>();
        }

        private async Task SendProjectAsync(EnvDTE.Project project, string remotePath, string filterString) {
            await Callbacks.WriteConsoleEx("Preparing to transfer project.\n", OutputType.Output, CancellationToken.None);

            var projectDir = Path.GetDirectoryName(project.FullName);
            var projectName = Path.GetFileNameWithoutExtension(project.FullName);
            var filter = new TransferFileFilter(filterString);

            await Callbacks.WriteConsoleEx($"Remote destination: {remotePath}\n", OutputType.Output, CancellationToken.None);
            await Callbacks.WriteConsoleEx($"File filter applied: {filterString}\n", OutputType.Output, CancellationToken.None);
            await Callbacks.WriteConsoleEx("Compressing project files for transfer:\n", OutputType.Output, CancellationToken.None);

            var compressedFilePath = await Task.Run(() => FileSystem.CompressDirectory(projectDir, (p) => {
                Callbacks.WriteConsoleEx($"Compressing: {p}\n", OutputType.Output, CancellationToken.None).Wait();
                return filter.Match(p);
            }));

            using (var fts = new FileTransferSession(Session, FileSystem)) {
                await Callbacks.WriteConsoleEx("Transferring project to remote host...", OutputType.Output, CancellationToken.None);
                var remoteFile = await fts.SendFileAsync(compressedFilePath);
                await Session.EvaluateAsync<string>(Invariant($"rtvs:::save_project({remoteFile.Id}, {projectName.ToRStringLiteral()}, '{remotePath.ToRPath()}')"), REvaluationKind.Normal);
            }

            await Callbacks.WriteConsoleEx(" Completed.\n", OutputType.Output, CancellationToken.None);
        }

        private async Task<string> GetStartupFileAsync(bool transferFiles, EnvDTE.Project project) {
            if (transferFiles && Session.IsRemoteSession) { // remote
                var projectName = Path.GetFileNameWithoutExtension(project.FullName);
                var remotePath = (await _properties.GetRemoteProjectPathAsync()).ToRPath();
                var startUpFile = (await _properties.GetStartupFileAsync()).ToRPath();
                return remotePath + projectName + "/" + startUpFile;
            } else { // local
                var projDir = Path.GetDirectoryName(project.FullName);
                var startUpFile = await _properties.GetStartupFileAsync();
                return Path.Combine(projDir, startUpFile);
            }
        }

        private string GetRemoteSettingsFile(string localSettingsPath, string localProjectPath, string remoteProjectPath) {
            return remoteProjectPath + localSettingsPath.Remove(0, localSettingsPath.Length);
        }

        private class TransferFileFilter {
            List<string> filterPatterns;
            public TransferFileFilter(string filterString) {
                string[] patterns = filterString.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                filterPatterns = new List<string>(patterns);
            }
            private static string[] separator = { ";"};

            public bool Match(string path) {
                var result = false;
                foreach (string pattern in filterPatterns) {
                    if (pattern == "*.*") {
                        result = true;
                    } else if (pattern.StartsWith("*.") || pattern.StartsWith("*.")) {
                        result = Path.GetExtension(path) == Path.GetExtension(pattern);
                    } else {
                        result = Path.GetFileName(path) == pattern;
                    }

                    if (result) {
                        break;
                    }
                }

                return result;
            }
        }
    }
}