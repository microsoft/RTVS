// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Core.Settings;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.R.Package.Options.Common;
using Microsoft.VisualStudio.R.Packages.R;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Options.R {
    [Export(typeof(IWritableSettingsStorage))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("Visual Studio R Editor settings")]
    [Order(Before = "Default")]
    internal sealed class VsRSettingsStorage : LanguageSettingsStorageWithDialog {
        public VsRSettingsStorage()
            : base(RGuidList.RLanguageServiceGuid, RGuidList.RPackageGuid, RPackage.OptionsDialogName) { }
    }
}
