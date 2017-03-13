// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.Controller;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Test.Shell {
    [ExcludeFromCodeCoverage]
    internal sealed class TestEditorShell : TestShellBase, IEditorShell {
        private static TestEditorShell _instance;
        private readonly TestServiceManager _serviceManager;

        private TestEditorShell(ICompositionCatalog catalog, Thread mainThread) {
            CompositionService = catalog.CompositionService;
            ExportProvider = catalog.ExportProvider;
            MainThread = mainThread;
            _serviceManager = new TestServiceManager(ExportProvider);
        }

        /// <summary>
        /// Called via reflection from CoreShell.TryCreateTestInstance
        /// in test scenarios
        /// </summary>
        public static void Create() {
            // Called via reflection in test cases. Creates instance
            // of the test shell that editor code can access during
            // test run.
            _instance = new TestEditorShell(EditorTestCompositionCatalog.Current, UIThreadHelper.Instance.Thread);
            EditorShell.Current = _instance;
        }

        public IServiceContainer GlobalServices => _serviceManager;

        #region ICompositionCatalog
        public ICompositionService CompositionService { get; private set; }
        public ExportProvider ExportProvider { get; private set; }
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
