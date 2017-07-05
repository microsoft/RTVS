// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.ProjectSystem.VS;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.SurveyNews;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Shell.Interop;
using IThreadHandling = Microsoft.VisualStudio.ProjectSystem.IProjectThreadingService;


namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    internal sealed class RProjectLoadHooks {
        private const string DefaultRDataName = ".RData";
        private const string DefaultRHistoryName = ".RHistory";

        [Export(typeof(IFileSystemMirroringProjectTemporaryItems))]
        private FileSystemMirroringProject Project { get; }

        private readonly MsBuildFileSystemWatcher _fileWatcher;
        private readonly string _projectDirectory;
        private readonly IRSettings _settings;
        private readonly IThreadHandling _threadHandling;
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly IEnumerable<Lazy<IVsProject>> _cpsIVsProjects;
        private readonly IProjectLockService _projectLockService;
        private readonly ICoreShell _coreShell;

        private IRInteractiveWorkflowVisual _workflow;
        private IRSession _session;
        private IRHistory _history;

        /// <summary>
        /// Backing field for the similarly named property.
        /// </summary>
        [ImportingConstructor]
        public RProjectLoadHooks(UnconfiguredProject unconfiguredProject
            , [ImportMany("Microsoft.VisualStudio.ProjectSystem.Microsoft.VisualStudio.Shell.Interop.IVsProject")] IEnumerable<Lazy<IVsProject>> cpsIVsProjects
            , IProjectLockService projectLockService
            , IThreadHandling threadHandling
            , [Import(AllowDefault = true)] IProjectItemDependencyProvider dependencyProvider
            , ICoreShell coreShell) {
            _unconfiguredProject = unconfiguredProject;
            _cpsIVsProjects = cpsIVsProjects;
            _projectLockService = projectLockService;

            _settings = coreShell.GetService<IRSettings>();
            _threadHandling = threadHandling;
            _coreShell = coreShell;

            _projectDirectory = unconfiguredProject.GetProjectDirectory();

            unconfiguredProject.ProjectRenamedOnWriter += ProjectRenamedOnWriter;
            unconfiguredProject.ProjectUnloading += ProjectUnloadingAsync;

            _fileWatcher = new MsBuildFileSystemWatcher(_projectDirectory, "*", 25, 1000, _coreShell.FileSystem(), new RMsBuildFileSystemFilter(), coreShell.Log());
            _fileWatcher.Error += FileWatcherError;
            Project = new FileSystemMirroringProject(unconfiguredProject, projectLockService, _fileWatcher, dependencyProvider, coreShell.Log());
        }

        [AppliesTo(ProjectConstants.RtvsProjectCapability)]
        [ProjectAutoLoad(startAfter: ProjectLoadCheckpoint.UnconfiguredProjectLocalCapabilitiesEstablished,
                         completeBy: ProjectLoadCheckpoint.BeforeLoadInitialConfiguration,
                         RequiresUIThread = false)]
        public async Task InitializeProjectFromDiskAsync() {
            await Project.CreateInMemoryImport();
            _fileWatcher.Start();

            await _threadHandling.SwitchToUIThread();
            // Make sure R package is loaded
            VsAppShell.EnsurePackageLoaded(RGuidList.RPackageGuid);

            // Verify project is not on a network share and give warning if it is
            CheckRemoteDrive(_projectDirectory);

            _workflow = _coreShell.GetService<IRInteractiveWorkflowVisualProvider>().GetOrCreate();
            _session = _workflow.RSession;
            _history = _workflow.History;

            if (_workflow.ActiveWindow == null) {
                var window = await _workflow.GetOrCreateVisualComponentAsync();
                window.Container.Show(focus: true, immediate: false);
            }

            try {
                await _session.HostStarted;
            } catch (Exception) {
                return;
            }

            _workflow.RSessions.BeforeDisposed += BeforeRSessionsDisposed;

            // TODO: need to compute the proper paths for remote, but they might not even exist if the project hasn't been deployed.
            // https://github.com/Microsoft/RTVS/issues/2223
            if (!_session.IsRemote) {
                var rdataPath = Path.Combine(_projectDirectory, DefaultRDataName);
                bool loadDefaultWorkspace = _coreShell.FileSystem().FileExists(rdataPath) && await GetLoadDefaultWorkspace(rdataPath);

                if (loadDefaultWorkspace) {
                    await _session.LoadWorkspaceAsync(rdataPath);
                }
                await _session.SetWorkingDirectoryAsync(_projectDirectory);
                _settings.WorkingDirectory = _projectDirectory;
            }

            _history.TryLoadFromFile(Path.Combine(_projectDirectory, DefaultRHistoryName));
            CheckSurveyNews();
        }

        private async void CheckSurveyNews() {
            // Don't return a task, the caller doesn't want to await on this
            // and hold up loading of the project.
            // We do it this way instead of calling DoNotWait extension in order
            // to handle any non critical exceptions.
            try {
                await _coreShell.GetService<ISurveyNewsService>().CheckSurveyNewsAsync(false);
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _coreShell.Log().Write(LogVerbosity.Normal, MessageCategory.Error, "SurveyNews exception: " + ex.Message);
            }
        }

        private void FileWatcherError(object sender, EventArgs args) {
            _fileWatcher.Error -= FileWatcherError;
            _coreShell.MainThread().Post(() => {
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

                    var solution = _coreShell.GetService<IVsSolution>(typeof(SVsSolution));
                    solution.CloseSolutionElement((uint)__VSSLNCLOSEOPTIONS.SLNCLOSEOPT_UnloadProject, (IVsHierarchy)iVsProject, 0);
                    return;
                }
            });
        }
        
        private async Task ProjectRenamedOnWriter(object sender, ProjectRenamedEventArgs args) {
            var oldImportName = FileSystemMirroringProjectUtilities.GetInMemoryTargetsFileName(args.OldFullPath);
            var newImportName = FileSystemMirroringProjectUtilities.GetInMemoryTargetsFileName(args.NewFullPath);
            using (var access = await _projectLockService.WriteLockAsync()) {
                await access.CheckoutAsync(_unconfiguredProject.FullPath);
                var xml = await access.GetProjectXmlAsync(_unconfiguredProject.FullPath);
                var import = xml.Imports.FirstOrDefault(i => i.Project.EqualsIgnoreCase(oldImportName));
                if (import != null) {
                    import.Project = newImportName;
                    import.Condition = $"Exists('{newImportName}')";
                    await Project.UpdateFullPathAsync(access);
                }
            }
        }

        private void BeforeRSessionsDisposed(object sender, EventArgs args) {
            _coreShell.Services.Tasks().Wait(ProjectUnloadingAsync(sender, args));
        }

        private async Task ProjectUnloadingAsync(object sender, EventArgs args) {
            await _coreShell.SwitchToMainThreadAsync(new CancellationTokenSource(10000).Token);

            _unconfiguredProject.ProjectRenamedOnWriter -= ProjectRenamedOnWriter;
            _unconfiguredProject.ProjectUnloading -= ProjectUnloadingAsync;
            _workflow.RSessions.BeforeDisposed -= BeforeRSessionsDisposed;

            _fileWatcher.Dispose();

            if (!_coreShell.FileSystem().DirectoryExists(_projectDirectory)) {
                return;
            }

            if (_settings.AlwaysSaveHistory) {
                _history.TrySaveToFile(Path.Combine(_projectDirectory, DefaultRHistoryName));
            }

            var rdataPath = Path.Combine(_projectDirectory, DefaultRDataName);
            var saveDefaultWorkspace = await GetSaveDefaultWorkspace(rdataPath);
            if (!_session.IsHostRunning) {
                return;
            }

            Task.Run(async () => {
                if (saveDefaultWorkspace) {
                    await _session.SaveWorkspaceAsync(rdataPath);
                }
                await _session.SetDefaultWorkingDirectoryAsync();
            }).SilenceException<RException>().DoNotWait();
        }

        private async Task<bool> GetLoadDefaultWorkspace(string rdataPath) {
            switch (_settings.LoadRDataOnProjectLoad) {
                case YesNoAsk.Yes:
                    return true;
                case YesNoAsk.Ask:
                    await _threadHandling.SwitchToUIThread();
                    return _coreShell.ShowMessage(
                        string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceIntoGlobalEnvironment, rdataPath),
                        MessageButtons.YesNo) == MessageButtons.Yes;
                default:
                    return false;
            }
        }

        private async Task<bool> GetSaveDefaultWorkspace(string rdataPath) {
            switch (_settings.SaveRDataOnProjectUnload) {
                case YesNoAsk.Yes:
                    return true;
                case YesNoAsk.Ask:
                    await _threadHandling.SwitchToUIThread();
                    return _coreShell.ShowMessage(
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
            if (remoteDrive) {
                _coreShell.ShowMessage(Resources.Warning_UncPath, MessageButtons.OK);
            }
        }
    }
}