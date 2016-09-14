// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Settings;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.Controller;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;

namespace Microsoft.Languages.Editor.Test.Shell {
    [ExcludeFromCodeCoverage]
    internal sealed class TestEditorShell : TestShellBase, IEditorShell {
        private static TestEditorShell _instance;

        private TestEditorShell(ICompositionCatalog catalog) {
            CompositionService = catalog.CompositionService;
            ExportProvider = catalog.ExportProvider;
            MainThread = Thread.CurrentThread;
        }

        /// <summary>
        /// Called via reflection from CoreShell.TryCreateTestInstance
        /// in test scenarios
        /// </summary>
        public static void Create() {
            // Called via reflection in test cases. Creates instance
            // of the test shell that editor code can access during
            // test run.
            UIThreadHelper.Instance.Invoke(() => {
                _instance = new TestEditorShell(EditorTestCompositionCatalog.Current);
                EditorShell.Current = (IEditorShell)(object)_instance;
            });
        }

        #region ICompositionCatalog
        public ICompositionService CompositionService { get; private set; }
        public ExportProvider ExportProvider { get; private set; }
        #endregion

        #region ICoreShell
        /// <summary>
        /// Displays error message in a host-specific UI
        /// </summary>
        public T GetGlobalService<T>(Type type = null) where T : class {
            throw new NotImplementedException();
        }

        public bool IsUnitTestEnvironment { get; set; } = true;
        public ITelemetryService TelemetryService { get; }
        public IntPtr ApplicationWindowHandle { get; }
        public IWritableSettingsStorage SettingsStorage => Substitute.For<IWritableSettingsStorage>();
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
    }
}
