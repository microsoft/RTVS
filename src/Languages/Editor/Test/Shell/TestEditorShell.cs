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
    internal sealed class Testshell : TestShellBase, ICoreShell {
        private static Testshell _instance;

        private Testshell(ICompositionCatalog catalog) :
            base(catalog.ExportProvider) {
        }

        /// <summary>
        /// Called via reflection from CoreShell.TryCreateTestInstance
        /// in test scenarios
        /// </summary>
        public static void Create() {
            // Called via reflection in test cases. Creates instance
            // of the test shell that editor code can access during
            // test run.
            _instance = new Testshell(EditorTestCompositionCatalog.Current, UIThreadHelper.Instance.Thread);
            _instance.Current = _instance;
        }

        #region ICoreShell
        #endregion
    }

    class TestAppEditorSupport : IApplicationEditorSupport {
        #region IApplicationEditorSupport
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
