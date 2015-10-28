using System.ComponentModel.Composition;
using Microsoft.Languages.Core.Settings;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.R.Package.Options.Common;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    [Export(typeof(IWritableSettingsStorage))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Editor settings")]
    [Order(Before = "Default")]
    internal sealed class VsRSettingsStorage : LanguageSettingsStorageWithDialog {
        public VsRSettingsStorage()
            : base(RGuidList.RLanguageServiceGuid, RGuidList.RPackageGuid, RPackage.OptionsDialogName) { }
    }
}
