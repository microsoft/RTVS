// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Testing;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Shell {
    /// <summary>
    /// Application shell provides access to services
    /// such as composition container, export provider, global VS IDE
    /// services and so on.
    /// </summary>
    [Export(typeof(ICoreShell))]
    public sealed partial class VsAppShell : ICoreShell, IVsShellPropertyEvents, IDisposable {
        private static VsAppShell _instance;

        public VsAppShell() {
            Debug.Assert(_instance == null, "VsAppShell is a singleton and cannot be created twice");
            _instance = this;
            _services = new VsServiceManager(this);
            ConfigureServices();
        }

        private VsAppShell(Func<VsAppShell, VsServiceManager> serviceManagerFactory) {
            Debug.Assert(_instance == null, "VsAppShell is a singleton and cannot be created twice");
            _services = serviceManagerFactory(this);
        }

        /// <summary>
        /// Current application shell instance. Provides access to services
        /// such as composition container, export provider, global VS IDE
        /// services and so on.
        /// </summary>
        public static ICoreShell Current {
            get {
                if (TestEnvironment.Current != null && _instance == null) {
                    SetupTestInstance();
                }

                return GetInstance();
            }
        }
    }
}
