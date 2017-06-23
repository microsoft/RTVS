// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Languages.Editor.Settings;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.R.Package.Editors;
using Microsoft.VisualStudio.R.Package.Options.Markdown;
using Microsoft.VisualStudio.R.Package.Options.R.Editor;
using Microsoft.VisualStudio.R.Package.Packages;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;
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
    [ProvideLanguageEditorOptionPage(typeof(RMarkdownOptionsDialog), MdContentTypeDefinition.LanguageName, "", "Advanced", "#20136")]
    internal sealed class MdPackage : BasePackage<MdLanguageService> {
        private readonly Dictionary<string, Type> _editorOptionsPages = new Dictionary<string, Type> {
            { "R Markdown", typeof(RMarkdownOptionsDialog)}
        };
        private static MdPackage _package;

        public static MdPackage Current {
            get {
                if(_package == null) {
                    VsAppShell.EnsurePackageLoaded(MdGuidList.MdPackageGuid);
                    Debug.Assert(_package != null);
                }
                return _package;
            }
        }

        public IEditorSettingsStorage LanguageSettingsStorage { get; private set; }

        protected override void Initialize() {
            _package = this;

            VsAppShell.EnsureInitialized();
            LoadEditorSettings();

            base.Initialize();
        }

        protected override IEnumerable<IVsEditorFactory> CreateEditorFactories() {
            yield return new MdEditorFactory(this, VsAppShell.Current.Services);
        }

        protected override object GetAutomationObject(string name) {
            if (_editorOptionsPages.TryGetValue(name, out Type pageType)) {
                var page = GetDialogPage(pageType);
                return page.AutomationObject;

            }
            return base.GetAutomationObject(name);
        }

        private void LoadEditorSettings() {
            var storage = new LanguageSettingsStorage(this, VsAppShell.Current.Services, MdGuidList.MdLanguageServiceGuid, _editorOptionsPages.Keys);
            LanguageSettingsStorage = storage;
            storage.Load();
        }
    }
}
