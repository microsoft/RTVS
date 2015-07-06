using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.ContentType;
using Microsoft.Languages.Editor.Settings;
using Microsoft.VisualStudio.R.Package.Options.Common;
using Microsoft.VisualStudio.R.Packages;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Options.R
{
    [Export(typeof(IWritableEditorSettingsStorage))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Editor settings")]
    [Order(Before = "Default")]
    internal sealed class VsRSettingsStorage : LanguageSettingsStorageWithDialog
    {
        public VsRSettingsStorage()
            : base(RGuidList.LanguageServiceGuid, RGuidList.PackageGuid, RPackage.OptionsDialogName)
        {
        }
    }
}
