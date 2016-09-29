// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Settings;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Settings;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    /// <summary>
    /// Replacement for VsAppShell in unit tests.
    /// Created via reflection by test code.
    /// </summary>
    [ExcludeFromCodeCoverage]
    sealed class TestVsAppShell : TestShellBase, IApplicationShell {
        private IServiceProvider _sp;
        private static TestVsAppShell _instance;
        private static object _shellLock = new object();
        private IWritableSettingsStorage _settingStorage;

        private TestVsAppShell() {
            CompositionService = VsTestCompositionCatalog.Current.CompositionService;
            ExportProvider = VsTestCompositionCatalog.Current.ExportProvider;
            MainThread = UIThreadHelper.Instance.Thread;
            _sp = new TestServiceProvider();
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
                        _instance = new TestVsAppShell();
                        RToolsSettings.Current = new TestRToolsSettings();

                        var batch = new CompositionBatch()
                            .AddValue<IRSettings>(RToolsSettings.Current)
                            .AddValue(RToolsSettings.Current)
                            .AddValue<ICoreShell>(_instance)
                            .AddValue<IEditorShell>(_instance)
                            .AddValue<IApplicationShell>(_instance)
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

        #region IApplicationShell
        public string BrowseForFileOpen(IntPtr owner, string filter, string initialPath = null, string title = null) {
            return null;
        }
        public string BrowseForFileSave(IntPtr owner, string filter, string initialPath = null, string title = null) {
            return null;
        }

        public override IWritableSettingsStorage SettingsStorage {
            get {
                if (_settingStorage == null) {
                    var ctrs = ExportProvider.GetExportedValue<IContentTypeRegistryService>();
                    var contentType = ctrs.GetContentType(RContentTypeDefinition.ContentType);
                    _settingStorage = ComponentLocatorForOrderedContentType<IWritableSettingsStorage>
                                            .FindFirstOrderedComponent(CompositionService, contentType);
                }
                return _settingStorage;
            }
        }

        #endregion

        #region IEditorShell
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
        public T GetGlobalService<T>(Type type = null) where T : class {
            return _sp.GetService(type ?? typeof(T)) as T;
        }

        public ITelemetryService TelemetryService => Substitute.For<ITelemetryService>();
        public IntPtr ApplicationWindowHandle => IntPtr.Zero;
        public IActionLog Logger => Substitute.For<IActionLog>();
        #endregion
    }
}
