// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.R.Debugger.Engine;
using Microsoft.R.Debugger.Engine.PortSupplier;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.Help.Functions;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Package.Registration;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.R.Package.DataInspect;
using Microsoft.VisualStudio.R.Package.Definitions;
using Microsoft.VisualStudio.R.Package.Help;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Logging;
using Microsoft.VisualStudio.R.Package.Options.R;
using Microsoft.VisualStudio.R.Package.Options.R.Editor;
using Microsoft.VisualStudio.R.Package.Packages;
using Microsoft.VisualStudio.R.Package.Plots;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.RPackages.Mirrors;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages.R {
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#7002", "#7003", RtvsProductInfo.VersionString, IconResourceID = 400)]
    [Guid(RGuidList.RPackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideEditorExtension(typeof(REditorFactory), RContentTypeDefinition.FileExtension, 0x32, NameResourceID = 106)]
    [ProvideLanguageExtension(RGuidList.RLanguageServiceGuidString, RContentTypeDefinition.FileExtension)]
    [ProvideEditorFactory(typeof(REditorFactory), 106, CommonPhysicalViewAttributes = 0x2, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(REditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideLanguageService(typeof(RLanguageService), RContentTypeDefinition.LanguageName, 106, ShowSmartIndent = true,
        ShowMatchingBrace = true, MatchBraces = true, MatchBracesAtCaret = true, ShowCompletion = true, EnableLineNumbers = true,
        EnableFormatSelection = true, DefaultToInsertSpaces = true, RequestStockColors = true)]
    [ShowBraceCompletion(RContentTypeDefinition.LanguageName)]
    [LanguageEditorOptionsAttribute(RContentTypeDefinition.LanguageName, 2, true, true)]
    [ProvideLanguageEditorOptionPage(typeof(REditorOptionsDialog), RContentTypeDefinition.LanguageName, "", "Advanced", "#20136")]
    [ProvideProjectFileGenerator(typeof(RProjectFileGenerator), RGuidList.CpsProjectFactoryGuidString, FileExtensions = RContentTypeDefinition.RStudioProjectExtension, DisplayGeneratorFilter = 300)]
    [DeveloperActivity(RContentTypeDefinition.LanguageName, RGuidList.RPackageGuidString, sortPriority: 9)]
    [ProvideCpsProjectFactory(RGuidList.CpsProjectFactoryGuidString, RContentTypeDefinition.LanguageName)]
    [ProvideOptionPage(typeof(RToolsOptionsPage), "R Tools", "Advanced", 20116, 20136, true)]
    [ProvideInteractiveWindow(RGuidList.ReplInteractiveWindowProviderGuidString, Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom, Window = ToolWindowGuids80.Outputwindow, DocumentLikeTool = true)]
    [ProvideToolWindow(typeof(PlotWindowPane), Style = VsDockStyle.Linked, Window = ToolWindowGuids80.SolutionExplorer)]
    [ProvideToolWindow(typeof(HelpWindowPane), Style = VsDockStyle.Linked, Window = ToolWindowGuids80.PropertiesWindow)]
    [ProvideToolWindow(typeof(HistoryWindowPane), Style = VsDockStyle.Linked, Window = ToolWindowGuids80.SolutionExplorer)]
    [ProvideDebugEngine(RContentTypeDefinition.LanguageName, null, typeof(AD7Engine), DebuggerGuids.DebugEngineString)]
    [ProvideDebugLanguage(RContentTypeDefinition.LanguageName, DebuggerGuids.LanguageGuidString, "{D67D5DB8-3D44-4105-B4B8-47AB1BA66180}", DebuggerGuids.DebugEngineString, DebuggerGuids.CustomViewerString)]
    [ProvideDebugPortSupplier("R Interactive sessions", typeof(RDebugPortSupplier), DebuggerGuids.PortSupplierString, typeof(RDebugPortPicker))]
    [ProvideComClass(typeof(RDebugPortPicker))]
    [ProvideComClass(typeof(AD7CustomViewer))]
    [ProvideToolWindow(typeof(VariableWindowPane), Style = VsDockStyle.Linked, Window = ToolWindowGuids80.SolutionExplorer)]
    [ProvideToolWindow(typeof(VariableGridWindowPane), Style = VsDockStyle.Linked, Window = ToolWindowGuids80.SolutionExplorer, Transient = true)]
    [ProvideNewFileTemplatesAttribute(RGuidList.MiscFilesProjectGuidString, RGuidList.RPackageGuidString, "#106", @"Templates\NewItem\")]
    internal class RPackage : BasePackage<RLanguageService>, IRPackage {
        public const string OptionsDialogName = "R Tools";

        private readonly Lazy<RInteractiveWindowProvider> _interactiveWindowProvider = Lazy.Create(() => new RInteractiveWindowProvider());
        private System.Threading.Tasks.Task _indexBuildingTask;
        private IDisposable _activeTextViewTrackerToken;

        public static IRPackage Current { get; private set; }

        public RInteractiveWindowProvider InteractiveWindowProvider => _interactiveWindowProvider.Value;

        protected override void Initialize() {
            Current = this;

            // Force app shell creation before everything else
            var shell = VsAppShell.Current;
            if(IsCommandLineMode()) {
                return;
            }

            CranMirrorList.Download();
            VerifyWebToolsInstalled();

            RtvsTelemetry.Initialize();

            using (var p = RPackage.Current.GetDialogPage(typeof(RToolsOptionsPage)) as RToolsOptionsPage) {
                p.LoadSettings();
            }

            base.Initialize();

            ReplShortcutSetting.Initialize();
            ProjectIconProvider.LoadProjectImages();
            LogCleanup.DeleteLogsAsync(DiagnosticLogs.DaysToRetain);

            _indexBuildingTask = FunctionIndex.BuildIndexAsync();

            InitializeActiveWpfTextViewTracker();
            System.Threading.Tasks.Task.Run(() => RtvsTelemetry.Current.ReportConfiguration());
        }

        protected override void Dispose(bool disposing) {
            if (_indexBuildingTask != null && !_indexBuildingTask.IsFaulted) {
                _indexBuildingTask.Wait(2000);
                _indexBuildingTask = null;
            }

            _activeTextViewTrackerToken?.Dispose();

            LogCleanup.Cancel();
            ReplShortcutSetting.Close();
            ProjectIconProvider.Close();

            RtvsTelemetry.Current.Dispose();

            using (var p = RPackage.Current.GetDialogPage(typeof(RToolsOptionsPage)) as RToolsOptionsPage) {
                p.SaveSettings();
            }

            base.Dispose(disposing);
        }

        protected override IEnumerable<IVsEditorFactory> CreateEditorFactories() {
            yield return new REditorFactory(this);
        }

        protected override IEnumerable<IVsProjectGenerator> CreateProjectFileGenerators() {
            yield return new RProjectFileGenerator();
        }

        protected override IEnumerable<IVsProjectFactory> CreateProjectFactories() {
            yield break;
        }

        protected override IEnumerable<MenuCommand> CreateMenuCommands() {
            return PackageCommands.GetCommands(VsAppShell.Current.ExportProvider);
        }

        protected override object GetAutomationObject(string name) {
            if (name == OptionsDialogName) {
                DialogPage page = GetDialogPage(typeof(REditorOptionsDialog));
                return page.AutomationObject;
            }

            return base.GetAutomationObject(name);
        }

        public T FindWindowPane<T>(Type t, int id, bool create) where T : ToolWindowPane {
            return this.FindWindowPane(t, id, create) as T;
        }

        protected override int CreateToolWindow(ref Guid toolWindowType, int id) {
            if (toolWindowType == RGuidList.ReplInteractiveWindowProviderGuid) {
                _interactiveWindowProvider.Value.Open(id, false);
                return VSConstants.S_OK;
            }

            return base.CreateToolWindow(ref toolWindowType, id);
        }

        private void InitializeActiveWpfTextViewTracker() {
            var activeTextViewTracker = VsAppShell.Current.ExportProvider.GetExportedValue<ActiveWpfTextViewTracker>();
            var shell = (IVsUIShell7)GetService(typeof(SVsUIShell));
            var cookie = shell.AdviseWindowFrameEvents(activeTextViewTracker);
            _activeTextViewTrackerToken = Disposable.Create(() => shell.UnadviseWindowFrameEvents(cookie));
        }

        private void VerifyWebToolsInstalled() {
            Guid htmlEditorPackage = new Guid("CF49EC7D-92B1-4BBD-9254-9CC13978E82E");
            var shell = VsAppShell.Current.GetGlobalService<IVsShell>(typeof(SVsShell));
            int installed;
            int hr = shell.IsPackageInstalled(ref htmlEditorPackage, out installed);
            if (hr != VSConstants.S_OK || installed == 0) {
                VsAppShell.Current.ShowErrorMessage(Package.Resources.Error_NoWebTools);
            }
        }

        private bool IsCommandLineMode() {
            var shell = VsAppShell.Current.GetGlobalService<IVsShell>(typeof(SVsShell));
            if (shell != null) {
                object value = null;
                shell.GetProperty((int)__VSSPROPID.VSSPROPID_IsInCommandLineMode, out value);
                return value is bool && (bool)value;
            }
            return false;
        }
    }
}
