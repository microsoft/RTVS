// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed  class VsEditorSupport : IApplicationEditorSupport {
        private readonly ICoreShell _coreShell;

        public VsEditorSupport(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        #region IApplicationEditorSupport 
        /// <summary>
        /// Provides shim that implements ICommandTarget over 
        /// application-specific command target. For example, 
        /// Visual Studio is using IOleCommandTarget.
        /// </summary>
        /// <param name="commandTarget">Command target</param>
        /// <returns>Web components compatible command target</returns>
        public ICommandTarget TranslateCommandTarget(ITextView textView, object commandTarget) {
            var managedCommandTarget = commandTarget as ICommandTarget;
            if (managedCommandTarget != null)
                return managedCommandTarget;

            var oleCommandTarget = commandTarget as IOleCommandTarget;
            if (oleCommandTarget != null)
                return new OleToCommandTargetShim(textView, oleCommandTarget);

            Debug.Fail("Unknown command taget type");
            return null;
        }

        public object TranslateToHostCommandTarget(ITextView textView, object commandTarget) {
            var oleToCommandTargetShim = commandTarget as OleToCommandTargetShim;
            if (oleToCommandTargetShim != null)
                return oleToCommandTargetShim.OleTarget;

            var managedCommandTarget = commandTarget as ICommandTarget;
            if (managedCommandTarget != null)
                return new CommandTargetToOleShim(textView, managedCommandTarget);

            var oleCommandTarget = commandTarget as IOleCommandTarget;
            if (oleCommandTarget != null)
                return oleCommandTarget;

            Debug.Fail("Unknown command taget type");
            return null;
        }

        /// <summary>
        /// Creates compound undo action
        /// </summary>
        /// <param name="textView">Text view</param>
        /// <param name="textBuffer">Text buffer</param>
        /// <returns>Undo action instance</returns>
        public ICompoundUndoAction CreateCompoundAction(ITextView textView, ITextBuffer textBuffer)
            => new CompoundUndoAction(textView, _coreShell, addRollbackOnCancel: true);
        #endregion
    }
}
