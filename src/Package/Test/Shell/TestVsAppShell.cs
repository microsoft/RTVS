// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Languages.Editor.Test.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Settings;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    /// <summary>
    /// Replacement for Vsshell in unit tests.
    /// Created via reflection by test code.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed class TestVsshell : TestShellBase, ICoreShell {
        private readonly VsTestServiceManager _serviceManager;
        private static TestVsshell _instance;
        private static readonly object _shellLock = new object();

        private TestVsshell(): base(VsTestCompositionCatalog.Current.ExportProvider) {
            CompositionService = VsTestCompositionCatalog.Current.CompositionService;
            ExportProvider = VsTestCompositionCatalog.Current.ExportProvider;
            MainThread = UIThreadHelper.Instance.Thread;
            _serviceManager = new VsTestServiceManager(ExportProvider);
        }

        public override IServiceContainer GlobalServices => _serviceManager;

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
                        _instance = new TestVsshell();
                        RToolsSettings.Current = new TestRToolsSettings();

                        var batch = new CompositionBatch()
                            .AddValue<IRSettings>(RToolsSettings.Current)
                            .AddValue(RToolsSettings.Current)
                            .AddValue<ICoreShell>(_instance)
                            .AddValue<ICoreShell>(_instance)
                            .AddValue<ICoreShell>(_instance)
                            .AddValue(_instance);
                        VsTestCompositionCatalog.Current.Container.Compose(batch);

                        Vsshell.Current = _instance;
                    }
                }
            });
        }

        #region ICompositionCatalog
        public ICompositionService CompositionService { get; private set; }
        public ExportProvider ExportProvider { get; private set; }
        #endregion

        #region ICoreShell
        public string BrowseForFileOpen(IntPtr owner, string filter, string initialPath = null, string title = null) {
            return null;
        }
        public string BrowseForFileSave(IntPtr owner, string filter, string initialPath = null, string title = null) {
            return null;
        }
        #endregion

        #region ICoreShell
        public ICommandTarget TranslateCommandTarget(ITextView textView, object commandTarget) {
            return commandTarget as ICommandTarget;
        }
        public object TranslateToHostCommandTarget(ITextView textView, object commandTarget) {
            return commandTarget;
        }
        public ICompoundUndoAction CreateCompoundAction(ITextView textView, ITextBuffer textBuffer) {
            return new CompoundUndoAction(textView, this, addRollbackOnCancel: false);
        }
        #endregion

        #region ICoreShell
        public ITelemetryService TelemetryService => Substitute.For<ITelemetryService>();
        public IntPtr ApplicationWindowHandle => IntPtr.Zero;
        public IActionLog Logger => Substitute.For<IActionLog>();
        #endregion
    }
}
