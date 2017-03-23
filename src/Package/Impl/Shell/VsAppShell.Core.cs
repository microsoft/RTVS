// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    /// <summary>
    /// Application shell core services
    /// </summary>
    public sealed partial class VsAppShell {
        #region ICoreShell
        public string ApplicationName => IsUnitTestEnvironment ? "RTVS_Test" : "RTVS";

        public int LocaleId { get; private set; } = 1033;
        
        /// <summary>
        /// Fires when host application has completed it's startup sequence
        /// </summary>
        public event EventHandler<EventArgs> Started;

        /// <summary>
        /// Fires when host application is terminating
        /// </summary>
        public event EventHandler<EventArgs> Terminating;

        public bool IsUnitTestEnvironment { get; private set; }
        #endregion

        private void ConfigureCore() {
            var hostLocale = ServiceProvider.GlobalProvider.GetService(typeof(SUIHostLocale)) as IUIHostLocale;
            uint lcid;
            hostLocale.GetUILocale(out lcid);
            LocaleId = (int)lcid;
        }
    }
}
