using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.R.Languages;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.R.Package.Editors;
using Microsoft.VisualStudio.R.Package.Packages;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(GuidList.MdPackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    #region Markdown
    [ProvideEditorExtension(typeof(MdEditorFactory), ".md", 0x32, NameResourceID = 107)]
    [ProvideEditorExtension(typeof(MdEditorFactory), ".markdown", 0x32, NameResourceID = 107)]
    [ProvideLanguageService(typeof(MdLanguageService), MdContentTypeDefinition.LanguageName, 107, ShowSmartIndent = false)]
    [ProvideEditorFactory(typeof(MdEditorFactory), 107, CommonPhysicalViewAttributes = 0x2, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(MdEditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    #endregion
    internal sealed class MdPackage : BasePackage<MdLanguageService>
    {
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override IEnumerable<IVsEditorFactory> CreateEditorFactories()
        {
            yield return new MdEditorFactory(this);
        }
    }
}
