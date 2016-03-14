// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [AppliesTo("RTools")]
    internal sealed class RProjectLoadHooks {
        private const string DefaultRDataName = ".RData";
        private const string DefaultRHistoryName = ".RHistory";

        [Export(typeof(IFileSystemMirroringProjectTemporaryItems))]
        private FileSystemMirroringProject Project { get; }

        private readonly MsBuildFileSystemWatcher _fileWatcher;
        private readonly string _projectDirectory;
        private readonly IRToolsSettings _toolsSettings;
        private readonly IFileSystem _fileSystem;
        private readonly IThreadHandling _threadHandling;
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly IEnumerable<Lazy<IVsProject>> _cpsIVsProjects;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;

        private IRInteractiveWorkflow _workflow;
        private IRSession _session;
        private IRHistory _history;

        /// <summary>
        /// Backing field for the similarly named property.
        /// </summary>
        [ImportingConstructor]
        public RProjectLoadHooks(UnconfiguredProject unconfiguredProject
            , [ImportMany("Microsoft.VisualStudio.ProjectSystem.Microsoft.VisualStudio.Shell.Interop.IVsProject")] IEnumerable<Lazy<IVsProject>> cpsIVsProjects
            , IProjectLockService projectLockService
            , IRInteractiveWorkflowProvider workflowProvider
            , IInteractiveWindowComponentContainerFactory componentContainerFactory
            , IRToolsSettings toolsSettings
            , IFileSystem fileSystem
            , IThreadHandling threadHandling) {

            _unconfiguredProject = unconfiguredProject;
            _cpsIVsProjects = cpsIVsProjects;
            _workflowProvider = workflowProvider;
            _componentContainerFactory = componentContainerFactory;

            _toolsSettings = toolsSettings;
            _fileSystem = fileSystem;
            _threadHandling = threadHandling;
            _projectDirectory = unconfiguredProject.GetProjectDirectory();

            unconfiguredProject.ProjectUnloading += ProjectUnloading;
            _fileWatcher = new MsBuildFileSystemWatcher(_projectDirectory, "*", 25, fileSystem, new RMsBuildFileSystemFilter());
            _fileWatcher.Error += FileWatcherError;
            Project = new FileSystemMirroringProject(unconfiguredProject, projectLockService, _fileWatcher);
        }

        [AppliesTo("RTools")]
        [UnconfiguredProjectAutoLoad2(completeBy: UnconfiguredProjectLoadCheckpoint.CapabilitiesEstablished)]
        public async Task InitializeProjectFromDiskAsync() {
            await Project.CreateInMemoryImport();
            _fileWatcher.Start();

            // Force REPL window up and continue only when it is shown
            await _threadHandling.SwitchToUIThread();

            // Make sure R package is loaded
            VsAppShell.EnsurePackageLoaded(RGuidList.RPackageGuid);

            // Verify project is not on a network share and give warning if it is
            CheckRemoteDrive(_projectDirectory);

            _workflow = _workflowProvider.GetOrCreate();
            _session = _workflow.RSession;
            _history = _workflow.History;

            if (_workflow.ActiveWindow == null) {
                var window = await _workflow.GetOrCreateVisualComponent(_componentContainerFactory);
                window.Container.Show(true);
            }

            try {
                await _session.HostStarted;
            } catch (Exception) {
                return;
            }

            var rdataPath = Path.Combine(_projectDirectory, DefaultRDataName);
            bool loadDefaultWorkspace = _fileSystem.FileExists(rdataPath) && await GetLoadDefaultWorkspace(rdataPath);

            using (var evaluation = await _session.BeginEvaluationAsync()) {
                if (loadDefaultWorkspace) {
                    await evaluation.LoadWorkspace(rdataPath);
                }

                await evaluation.SetWorkingDirectory(_projectDirectory);
            }

            _toolsSettings.WorkingDirectory = _projectDirectory;
            _history.TryLoadFromFile(Path.Combine(_projectDirectory, DefaultRHistoryName));
        }

        private void FileWatcherError(object sender, EventArgs args) {
            _fileWatcher.Error -= FileWatcherError;
            VsAppShell.Current.DispatchOnUIThread(() => {
                foreach (var iVsProjectLazy in _cpsIVsProjects) {
                    IVsProject iVsProject;
                    try {
                        iVsProject = iVsProjectLazy.Value;
                    } catch (Exception) {
                        continue;
                    }

                    if (iVsProject.AsUnconfiguredProject() != _unconfiguredProject) {
                        continue;
                    }

                    var solution = VsAppShell.Current.GetGlobalService<IVsSolution>(typeof (SVsSolution));
                    solution.CloseSolutionElement((uint) __VSSLNCLOSEOPTIONS.SLNCLOSEOPT_UnloadProject, (IVsHierarchy)iVsProject, 0);
                    return;
                }
            });
        }

        private async Task ProjectUnloading(object sender, EventArgs args) {
            _unconfiguredProject.ProjectUnloading -= ProjectUnloading;
            _fileWatcher.Dispose();
            if (!_session.IsHostRunning) {
                return;
            }

            if (!_fileSystem.DirectoryExists(_projectDirectory)) {
                return;
            }

            var rdataPath = Path.Combine(_projectDirectory, DefaultRDataName);
            var saveDefaultWorkspace = await GetSaveDefaultWorkspace(rdataPath);

            Task.Run(async () => {
                try {
                    using (var evaluation = await _session.BeginEvaluationAsync()) {
                        if (saveDefaultWorkspace) {
                            await evaluation.SaveWorkspace(rdataPath);
                        }
                        await evaluation.SetDefaultWorkingDirectory();
                    }
                } catch (OperationCanceledException) {
                    return;
                }

                if (saveDefaultWorkspace || _toolsSettings.AlwaysSaveHistory) {
                    await _threadHandling.SwitchToUIThread();
                    _history.TrySaveToFile(Path.Combine(_projectDirectory, DefaultRHistoryName));
                }
            }).DoNotWait();
        }

        private async Task<bool> GetLoadDefaultWorkspace(string rdataPath) {
            switch (_toolsSettings.LoadRDataOnProjectLoad) {
                case YesNoAsk.Yes:
                    return true;
                case YesNoAsk.Ask:
                    await _threadHandling.SwitchToUIThread();
                    return VsAppShell.Current.ShowMessage(
                        string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceIntoGlobalEnvironment, rdataPath),
                        MessageButtons.YesNo) == MessageButtons.Yes;
                default:
                    return false;
            }
        }

        private async Task<bool> GetSaveDefaultWorkspace(string rdataPath) {
            switch (_toolsSettings.SaveRDataOnProjectUnload) {
                case YesNoAsk.Yes:
                    return true;
                case YesNoAsk.Ask:
                    await _threadHandling.SwitchToUIThread();
                    return VsAppShell.Current.ShowMessage(
                        string.Format(CultureInfo.CurrentCulture, Resources.SaveWorkspaceOnProjectUnload, rdataPath),
                        MessageButtons.YesNo) == MessageButtons.Yes;
                default:
                    return false;
            }
        }

        private void CheckRemoteDrive(string path) {
            bool remoteDrive = NativeMethods.PathIsUNC(path);
            if (!remoteDrive) {
                var pathRoot = Path.GetPathRoot(path);
                var driveType = (NativeMethods.DriveType)NativeMethods.GetDriveType(pathRoot);
                remoteDrive = driveType == NativeMethods.DriveType.Remote;
            }
            if(remoteDrive) {
                VsAppShell.Current.ShowMessage(Resources.Warning_UncPath, MessageButtons.OK);
            }
        }
    }
}