// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    /// <summary>
    /// Application shell core services
    /// </summary>
    public sealed partial class VsAppShell {
        #region ICoreShell
        public string ApplicationName => "RTVS";

        public int LocaleId { get; private set; }
        #endregion

        private void ConfigureCore() {
            var hostLocale = ServiceProvider.GlobalProvider.GetService(typeof(SUIHostLocale)) as IUIHostLocale;
            uint lcid;
            hostLocale.GetUILocale(out lcid);
            LocaleId = (int)lcid;
        }
    }
}
