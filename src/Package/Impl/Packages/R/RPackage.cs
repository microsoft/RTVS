using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.R.Debugger.Engine;
using Microsoft.R.Debugger.Engine.PortSupplier;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Package.Registration;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;
using Microsoft.VisualStudio.R.Package.DataInspect;
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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages.R {
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(RGuidList.RPackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideEditorExtension(typeof(REditorFactory), ".r", 0x32, NameResourceID = 106)]
    [ProvideEditorFactory(typeof(REditorFactory), 106, CommonPhysicalViewAttributes = 0x2, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(REditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideLanguageService(typeof(RLanguageService), RContentTypeDefinition.LanguageName, 106, ShowSmartIndent = true,
        ShowMatchingBrace = true, MatchBraces = true, MatchBracesAtCaret = true, ShowCompletion = true,
        EnableFormatSelection = true, DefaultToInsertSpaces = true)]
    [ShowBraceCompletion(RContentTypeDefinition.LanguageName)]
    [DefaultIndentAttribute(RContentTypeDefinition.LanguageName, 2, true)]
    [ProvideLanguageEditorOptionPage(typeof(REditorOptionsDialog), RContentTypeDefinition.LanguageName, "", "Advanced", "#20136")]
    [ProvideProjectFileGenerator(typeof(RProjectFileGenerator), RGuidList.CpsProjectFactoryGuidString, FileExtensions = RContentTypeDefinition.RStudioProjectExtension, DisplayGeneratorFilter = 300)]
    [DeveloperActivity("R", RGuidList.RPackageGuidString, sortPriority: 9)]
    [ProvideCpsProjectFactory(RGuidList.CpsProjectFactoryGuidString, RContentTypeDefinition.LanguageName)]
    [ProvideOptionPage(typeof(RToolsOptionsPage), "R Tools", "Advanced", 20116, 20136, true)]
    [ProvideInteractiveWindow(RGuidList.ReplInteractiveWindowProviderGuidString, Style = VsDockStyle.Tabbed, Orientation = ToolWindowOrientation.Bottom, Window = ToolWindowGuids80.Outputwindow, DocumentLikeTool = true)]
    //[ProvideToolWindow(typeof(PlotWindowPane), Style = VsDockStyle.Linked, Window = ToolWindowGuids80.SolutionExplorer)]
    [ProvideDebugEngine(RContentTypeDefinition.LanguageName, null, typeof(AD7Engine), DebuggerGuids.DebugEngineString)]
    [ProvideDebugLanguage(RContentTypeDefinition.LanguageName, RGuidList.RLanguageServiceGuidString, "{D67D5DB8-3D44-4105-B4B8-47AB1BA66180}", DebuggerGuids.DebugEngineString)]
    [ProvideDebugPortSupplier("R Interactive sessions", typeof(RDebugPortSupplier), DebuggerGuids.PortSupplierString, typeof(RDebugPortPicker))]
    [ProvideDebugPortPicker(typeof(RDebugPortPicker))]
    [ProvideToolWindow(typeof(VariableWindowPane), Style = VsDockStyle.Linked, Window = ToolWindowGuids80.SolutionExplorer)]
    internal class RPackage : BasePackage<RLanguageService> {
        public const string OptionsDialogName = "R Tools";

        private readonly Lazy<RInteractiveWindowProvider> _interactiveWindowProvider = new Lazy<RInteractiveWindowProvider>(() => new RInteractiveWindowProvider());
        private System.Threading.Tasks.Task _indexBuildingTask;

        public static RPackage Current { get; private set; }

        public RInteractiveWindowProvider InteractiveWindowProvider => _interactiveWindowProvider.Value;

        protected override void Initialize() {
            Current = this;

            CranMirrorList.Download();

            base.Initialize();

            RToolsSettings.Init(AppShell.Current.ExportProvider);
            ReplShortcutSetting.Initialize();
            ProjectIconProvider.LoadProjectImages();
            LogCleanup.DeleteLogsAsync(DiagnosticLogs.DaysToRetain);

            _indexBuildingTask = FunctionIndex.BuildIndexAsync();
        }

        protected override void Dispose(bool disposing) {
            if (_indexBuildingTask != null && !_indexBuildingTask.IsFaulted) {
                _indexBuildingTask.Wait(2000);
                _indexBuildingTask = null;
            }

            LogCleanup.Cancel();
            ReplShortcutSetting.Close();
            ProjectIconProvider.Close();
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
            return PackageCommands.GetCommands(AppShell.Current.ExportProvider);
        }

        protected override object GetAutomationObject(string name) {
            if (name == OptionsDialogName) {
                DialogPage page = GetDialogPage(typeof(REditorOptionsDialog));
                return page.AutomationObject;
            }

            return base.GetAutomationObject(name);
        }

        protected override int CreateToolWindow(ref Guid toolWindowType, int id) {
            if (toolWindowType == RGuidList.ReplInteractiveWindowProviderGuid) {
                IVsInteractiveWindow result = _interactiveWindowProvider.Value.Create(id);
                return result != null ? VSConstants.S_OK : VSConstants.E_FAIL;
            }

            return base.CreateToolWindow(ref toolWindowType, id);
        }
    }
}
