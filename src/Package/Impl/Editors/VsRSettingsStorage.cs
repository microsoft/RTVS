// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Core.Settings;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Editors {
    [Export(typeof(IWritableEditorSettingsStorage))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("Visual Studio R Editor settings")]
    [Order(Before = "Default")]
    internal sealed class VsREditorSettingsStorage : LanguageSettingsStorageWithDialog {
        public VsREditorSettingsStorage()
            : base(RGuidList.RLanguageServiceGuid, RGuidList.RPackageGuid, RPackage.OptionsDialogName) { }
    }
}
