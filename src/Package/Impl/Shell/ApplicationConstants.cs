// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    [Export(typeof(IApplicationConstants))]
    sealed class ApplicationConstants : IApplicationConstants {
        private readonly IApplicationShell _shell;

        [ImportingConstructor]
        public ApplicationConstants(IApplicationShell shell) {
            _shell = shell;
        }

        /// <summary>
        /// Returns host locale ID
        /// </summary>
        public int LocaleId {
            get {
                var hostLocale = _shell.GetGlobalService<IUIHostLocale>(typeof(SUIHostLocale));
                uint lcid;
                if (hostLocale != null && hostLocale.GetUILocale(out lcid) == VSConstants.S_OK) {
                    return (int)lcid;
                }
                return 0;
            }
        }

        /// <summary>
        /// Hive under HKLM that can be used by the system administrator to control
        /// certain application functionality. For example, security and privacy related
        /// features such as level of logging permitted.
        /// </summary>
        public string LocalMachineHive => @"Software\Microsoft\R Tools";
    }
}
