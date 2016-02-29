// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Options.Common {
    /// <summary>
    /// Base class of VS settings that also expose Tools | Options page that is derived from DialogPage.
    /// <seealso cref="WebSettingsStorage"/>
    /// </summary>
    public abstract class LanguageSettingsStorageWithDialog : LanguageSettingsStorage {
        private Guid _packageGuid;
        private IEnumerable<string> _automationObjectNames;

        protected LanguageSettingsStorageWithDialog(Guid languageServiceId, Guid packageId, string automationObjectName)
            : this(languageServiceId, packageId, new string[] { automationObjectName }) { }

        protected LanguageSettingsStorageWithDialog(Guid languageServiceId, Guid packageId, IEnumerable<string> automationObjectNames)
            : base(languageServiceId) {
            _packageGuid = packageId;
            _automationObjectNames = automationObjectNames;
        }

        /// <summary>
        /// Loads settings via language (editor) tools options page
        /// </summary>
        public override void LoadFromStorage() {
            IVsShell shell = VsAppShell.Current.GetGlobalService<IVsShell>(typeof(SVsShell));
            if (shell != null) {
                IVsPackage package;
                shell.LoadPackage(ref _packageGuid, out package);
                Debug.Assert(package != null);

                if (package != null) {
                    foreach (string curAutomationObjectName in _automationObjectNames) {
                        object automationObject = null;
                        package.GetAutomationObject(curAutomationObjectName, out automationObject);
                        Debug.Assert(automationObject is DialogPage);
                    }
                }
            }
        }
    }
}
