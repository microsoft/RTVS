using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Settings;
using Microsoft.R.Visualizer;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Package.Registration;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;
using Microsoft.VisualStudio.R.Languages;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.R.Package.Editors;
using Microsoft.VisualStudio.R.Package.Options.R;
using Microsoft.VisualStudio.R.Package.Options.R.Editor;
using Microsoft.VisualStudio.R.Package.Packages;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(GuidList.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideProjectFileGenerator(typeof(RProjectFileGenerator), GuidList.CpsProjectFactoryGuidString, FileExtensions = RContentTypeDefinition.RStudioProjectExtension, DisplayGeneratorFilter = 300)]
    [ProvideEditorExtension(typeof(REditorFactory), ".r", 0x32, NameResourceID = 106)]
    [ProvideEditorFactory(typeof(REditorFactory), 20136, CommonPhysicalViewAttributes = 0x2, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(REditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideOptionPage(typeof(RToolsOptionsPage), "R Tools", "Advanced", 20116, 20136, true)]
    [ProvideLanguageService(typeof(RLanguageService), RContentTypeDefinition.LanguageName, 106, ShowSmartIndent = true)]
    [ProvideLanguageEditorOptionPage(typeof(REditorOptionsDialog), RContentTypeDefinition.LanguageName, "", "Advanced", "#20136")]
    [ProvideCpsProjectFactory(GuidList.CpsProjectFactoryGuidString, RContentTypeDefinition.LanguageName)]
    [ProvideInteractiveWindow(GuidList.ReplWindowGuidString, Style = VsDockStyle.Linked, Orientation = ToolWindowOrientation.Bottom, Window = ToolWindowGuids80.Outputwindow, DocumentLikeTool = true)]
    [ProvideToolWindow(typeof(PlotWindowPane), Style = VsDockStyle.Linked, Window = ToolWindowGuids80.SolutionExplorer)]
    internal sealed class RPackage : BasePackage<RLanguageService>
    {
        public const string OptionsDialogName = "R Tools";

        protected override void Initialize()
        {
            base.Initialize();

            IComponentModel componentModel = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel)) as IComponentModel;
            RToolsSettings.VerifyRIsInstalled(componentModel.DefaultExportProvider);

            FunctionIndex.BuildIndexAsync();
        }

        protected override void Dispose(bool disposing)
        {
            //FunctionIndex.SaveIndexAsync();
            base.Dispose(disposing);
        }

        protected override IEnumerable<IVsEditorFactory> CreateEditorFactories()
        {
            yield return new REditorFactory(this);
        }

        protected override IEnumerable<IVsProjectGenerator> CreateProjectFileGenerators()
        {
            yield return new RProjectFileGenerator();
        }

        protected override IEnumerable<IVsProjectFactory> CreateProjectFactories()
        {
            yield break;
        }

        protected override IEnumerable<MenuCommand> CreateMenuCommands()
        {
            yield return new MenuCommand(
                (sender, args) => GetInteractiveWindowProvider().Open(instanceId: 0, focus: true),
                new CommandID(GuidList.RInteractiveCommandSetGuid, 0x0100));

            // TODO: abstract the pane. reference to PTVS
            yield return new MenuCommand(
                (sender, args) => ShowWindowPane(typeof(PlotWindowPane), true),
                new CommandID(GuidList.PlotWindowGuid, 0x0100));
        }

        protected override object GetAutomationObject(string name)
        {
            if (name == RPackage.OptionsDialogName)
            {
                DialogPage page = GetDialogPage(typeof(REditorOptionsDialog));
                return page.AutomationObject;
            }

            return base.GetAutomationObject(name);
        }

        protected override int CreateToolWindow(ref Guid toolWindowType, int id)
        {
            if (toolWindowType == GuidList.ReplWindowGuid)
            {
                var result = GetInteractiveWindowProvider().Create(id);
                return result != null ? VSConstants.S_OK : VSConstants.E_FAIL;
            }

            return base.CreateToolWindow(ref toolWindowType, id);
        }

        private static IVsInteractiveWindowProvider GetInteractiveWindowProvider()
        {
            return AppShell.Current.ExportProvider.GetExportedValue<IVsInteractiveWindowProvider>();
        }

        private void ShowWindowPane(Type windowType, bool focus)
        {
            var window = FindWindowPane(windowType, 0, true) as ToolWindowPane;
            if (window != null)
            {
                var frame = window.Frame as IVsWindowFrame;
                if (frame != null)
                {
                    ErrorHandler.ThrowOnFailure(frame.Show());
                }
                if (focus)
                {
                    var content = window.Content as System.Windows.UIElement;
                    if (content != null)
                    {
                        content.Focus();
                    }
                }
            }
        }
    }
}