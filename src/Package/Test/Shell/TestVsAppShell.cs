// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Test.Shell;
using Microsoft.R.Components.Settings;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    /// <summary>
    /// Replacement for Vsshell in unit tests.
    /// Created via reflection by test code.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed class TestVsShell : TestShellBase, ICompositionCatalog {
        private static TestVsShell _instance;
        private static readonly object _shellLock = new object();

        private TestVsShell(): base(VsTestCompositionCatalog.Current.ExportProvider) {
            CompositionService = VsTestCompositionCatalog.Current.CompositionService;
            ExportProvider = VsTestCompositionCatalog.Current.ExportProvider;
        }

        public static void Create() {
            // Called via reflection in test cases. Creates instance
            // of the test shell that code can access during the test run.
            // other shell objects may choose to create their own
            // replacements. For example, interactive editor tests
            // need smaller MEF catalog which excludes certain 
            // VS-specific implementations.
            UIThreadHelper.Instance.Invoke(() => {
                lock (_shellLock) {
                    if (_instance == null){
                        _instance = new TestVsShell();
                        RToolsSettings.Current = new TestRToolsSettings();

                        var batch = new CompositionBatch()
                            .AddValue<IRSettings>(RToolsSettings.Current)
                            .AddValue(RToolsSettings.Current)
                            .AddValue<ICoreShell>(_instance)
                            .AddValue(_instance);
                        VsTestCompositionCatalog.Current.Container.Compose(batch);

                        VsAppShell.Current = _instance;
                    }
                }
            });
        }

        #region ICompositionCatalog
        public ICompositionService CompositionService { get; private set; }
        public ExportProvider ExportProvider { get; private set; }
        #endregion
    }
}
