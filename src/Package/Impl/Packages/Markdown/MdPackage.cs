// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.R.Package.Packages;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Packages.Markdown {
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(MdGuidList.MdPackageGuidString)]
    [ProvideLanguageExtension(MdGuidList.MdLanguageServiceGuidString, MdContentTypeDefinition.FileExtension)]
    [ProvideEditorExtension(typeof(MdEditorFactory), ".rmd", 0x32, NameResourceID = 107)]
    [ProvideLanguageService(typeof(MdLanguageService), MdContentTypeDefinition.LanguageName, 107, ShowSmartIndent = false)]
    [ProvideEditorFactory(typeof(MdEditorFactory), 107, CommonPhysicalViewAttributes = 0x2, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    [ProvideEditorLogicalView(typeof(MdEditorFactory), VSConstants.LOGVIEWID.TextView_string)]
    internal sealed class MdPackage : BasePackage<MdLanguageService> {

        protected override void Initialize() {
            VsAppShell.EnsureInitialized();
            base.Initialize();
        }

        protected override IEnumerable<IVsEditorFactory> CreateEditorFactories() {
            yield return new MdEditorFactory(this);
        }
    }
}
