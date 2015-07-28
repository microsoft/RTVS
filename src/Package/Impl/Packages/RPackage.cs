using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Languages.Editor.ContentType;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Package.Registration;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Shell;
using Microsoft.VisualStudio.R.Languages;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.R.Package.Editors;
using Microsoft.VisualStudio.R.Package.Options.R;
using Microsoft.VisualStudio.R.Package.Packages;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(RGuidList.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideProjectFileGenerator(typeof(RProjectFileGenerator), RGuidList.CpsProjectFactoryGuidString, FileExtensions = RContentTypeDefinition.RStudioProjectExtension, DisplayGeneratorFilter = 300)]
    [ProvideEditorExtension(typeof(REditorFactory), ".r", 0x50, NameResourceID = 106)]
    [ProvideEditorFactory(typeof(REditorFactory), 106, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(REditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    [ProvideOptionPage(typeof(RToolsOptionsPage), "R Tools", "Advanced", 20116, 20136, true)]
    [ProvideLanguageEditorOptionPage(typeof(REditorOptionsDialog), RContentTypeDefinition.LanguageName, "", "Advanced", "#106")]
    [ProvideCpsProjectFactory(RGuidList.CpsProjectFactoryGuidString, RContentTypeDefinition.LanguageName)]
    //[ProvideOptionPage(typeof(ROptionsPage), "R Language Category", "Engine", 1000, 1001, true)]
    internal sealed class RPackage : BasePackage<RLanguageService>
    {
        public const string OptionsDialogName = "R Tools";
        
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
    }

}
