// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Debugger;
using Microsoft.R.Debugger.PortSupplier;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Extensions;
using Microsoft.R.Host.Client.Host;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;


namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    // ExportDebugger must match rule name in ..\Rules\Debugger.xaml.
    [ExportDebugger("RDebugger")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal class RDebugLaunchProvider : DebugLaunchProviderBase {
        private readonly ProjectProperties _properties;
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IProjectSystemServices _pss;
        private readonly IFileSystem _fs;

        [ImportingConstructor]
        public RDebugLaunchProvider(ConfiguredProject configuredProject, IRInteractiveWorkflowVisualProvider interactiveWorkflowProvider, IProjectSystemServices pss)
            : base(configuredProject) {
            _properties = configuredProject.Services.ExportProvider.GetExportedValue<ProjectProperties>();
            _interactiveWorkflow = interactiveWorkflowProvider.GetOrCreate();
            _pss = pss;
            _fs = _interactiveWorkflow.Shell.FileSystem();
        }

        private IFileSystem FileSystem => _fs;

        private IRSession Session => _interactiveWorkflow.RSession;

        private IConsole Console => _interactiveWorkflow.Console;

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

            _interactiveWorkflow.ActiveWindow?.Container.Show(focus: false, immediate: false);

            bool transferFiles = await _properties.GetTransferProjectOnRunAsync();
            string remotePath = await _properties.GetRemoteProjectPathAsync();
            string filterString = await _properties.GetFileFilterAsync();

            var activeProject = _pss.GetActiveProject();
            if (transferFiles && Session.IsRemote && activeProject != null) {
                await SendProjectAsync(activeProject, remotePath, filterString, CancellationToken.None);
            }

            // user must set the path for local or remote cases
            var startupFile = await GetStartupFileAsync(transferFiles, activeProject);

            if (string.IsNullOrWhiteSpace(startupFile)) {
                Console.WriteErrorLine(Resources.Launch_NoStartupFile);
                return;
            }
            await SourceFileAsync(transferFiles, startupFile, $"{Resources.Launch_StartupFileDoesNotExist} {startupFile}");

            var settingsFile = await _properties.GetSettingsFileAsync();
            if (!string.IsNullOrWhiteSpace(settingsFile)) {
                if (activeProject != null) {
                    var dirPath = Path.GetDirectoryName(activeProject.FullName);
                    settingsFile = settingsFile.MakeAbsolutePathFromRRelative(dirPath);
                    if (FileSystem.FileExists(settingsFile)) {
                        if (Session.IsRemote) {
                            var remoteSettingsPath = GetRemoteSettingsFile(settingsFile, dirPath, remotePath);
                            await SourceFileAsync(transferFiles, remoteSettingsPath, $"{Resources.Launch_SettingsFileDoesNotExist} {settingsFile}");
                        } else {
                            await SourceFileAsync(transferFiles, settingsFile, $"{Resources.Launch_SettingsFileDoesNotExist} {settingsFile}");
                        }
                    }
                }
            }
        }

        private async Task SourceFileAsync(bool transferFiles, string file, string errorMessage) {
            bool fileExists = false;
            if (transferFiles && Session.IsRemote) {
                try {
                    fileExists = await Session.EvaluateAsync<bool>($"file.exists({file.ToRPath().ToRStringLiteral()})", REvaluationKind.Normal);
                } catch (RHostDisconnectedException rhdex) {
                    Console.WriteLine(Resources.Error_UnableToVerifyFile.FormatInvariant(rhdex.Message));
                }
            } else {
                fileExists = FileSystem.FileExists(file);
            }

            if (!fileExists) {
                Console.WriteErrorLine(errorMessage);
                return;
            }

            Console.WriteLine(string.Format(Resources.Info_SourcingFile, file));
            await _interactiveWorkflow.Operations.SourceFileAsync(file, echo: false).SilenceException<Exception>();
        }

        private async Task SendProjectAsync(EnvDTE.Project project, string remotePath, string filterString, CancellationToken cancellationToken) {
            Console.WriteLine(Resources.Info_PreparingProjectForTransfer);

            var projectDir = Path.GetDirectoryName(project.FullName);
            var projectName = Path.GetFileNameWithoutExtension(project.FullName);

            string[] filterSplitter = { ";" };
            Matcher matcher = new Matcher(StringComparison.InvariantCultureIgnoreCase);
            matcher.AddIncludePatterns(filterString.Split(filterSplitter, StringSplitOptions.RemoveEmptyEntries));

            Console.WriteLine(Resources.Info_RemoteDestination.FormatInvariant(remotePath));
            Console.WriteLine(Resources.Info_FileTransferFilter.FormatInvariant(filterString));
            Console.WriteLine(Resources.Info_CompressingFiles);

            var compressedFilePath = FileSystem.CompressDirectory(projectDir, matcher, new Progress<string>((p) => {
                Console.WriteLine(Resources.Info_LocalFilePath.FormatInvariant(p));
                string dest = p.MakeRelativePath(projectDir).ProjectRelativePathToRemoteProjectPath(remotePath, projectName);
                Console.WriteLine(Resources.Info_RemoteFilePath.FormatInvariant(dest));
            }), CancellationToken.None);
            
            using (var fts = new DataTransferSession(Session, FileSystem)) {
                Console.WriteLine(Resources.Info_TransferringFiles);
                var remoteFile = await fts.SendFileAsync(compressedFilePath, true, null, cancellationToken);
                await Session.EvaluateAsync<string>($"rtvs:::save_to_project_folder({remoteFile.Id}, {projectName.ToRStringLiteral()}, '{remotePath.ToRPath()}')", REvaluationKind.Normal, cancellationToken);
            }

            Console.WriteLine(Resources.Info_TransferringFilesDone);
        }

        private async Task<string> GetStartupFileAsync(bool transferFiles, EnvDTE.Project project) {
            if (transferFiles && Session.IsRemote) { // remote
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

    }
}