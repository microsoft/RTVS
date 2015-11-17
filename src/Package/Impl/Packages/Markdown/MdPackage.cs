using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.R.Package.Packages;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages.Markdown {
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(MdGuidList.MdPackageGuidString)]
    [ProvideLanguageExtension(MdGuidList.MdLanguageServiceGuidString, MdContentTypeDefinition.FileExtension1)]
    [ProvideLanguageExtension(MdGuidList.MdLanguageServiceGuidString, MdContentTypeDefinition.FileExtension2)]
    [ProvideLanguageExtension(MdGuidList.MdLanguageServiceGuidString, MdContentTypeDefinition.FileExtension3)]
    [ProvideEditorExtension(typeof(MdEditorFactory), ".md", 0x32, NameResourceID = 107)]
    [ProvideEditorExtension(typeof(MdEditorFactory), ".rmd", 0x32, NameResourceID = 107)]
    [ProvideEditorExtension(typeof(MdEditorFactory), ".markdown", 0x32, NameResourceID = 107)]
    [ProvideLanguageService(typeof(MdLanguageService), MdContentTypeDefinition.LanguageName, 107, ShowSmartIndent = false)]
    [ProvideEditorFactory(typeof(MdEditorFactory), 107, CommonPhysicalViewAttributes = 0x2, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(MdEditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    internal sealed class MdPackage : BasePackage<MdLanguageService> {
        protected override IEnumerable<IVsEditorFactory> CreateEditorFactories() {
            yield return new MdEditorFactory(this);
        }
    }
}
