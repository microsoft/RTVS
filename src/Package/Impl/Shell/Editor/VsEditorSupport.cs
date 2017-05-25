// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.Undo;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.R.Package.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed  class VsEditorSupport : IEditorSupport {
        private readonly IServiceContainer _services;

        public VsEditorSupport(IServiceContainer services) {
            _services = services;
        }

        #region IEditorSupport 
        /// <summary>
        /// Provides shim that implements ICommandTarget over 
        /// application-specific command target. For example, 
        /// Visual Studio is using IOleCommandTarget.
        /// </summary>
        /// <param name="editorView">Editor view</param>
        /// <param name="commandTarget">Command target</param>
        /// <returns>Web components compatible command target</returns>
        public ICommandTarget TranslateCommandTarget(IEditorView editorView, object commandTarget) {
            var managedCommandTarget = commandTarget as ICommandTarget;
            if (managedCommandTarget != null) {
                return managedCommandTarget;
            }
            var oleCommandTarget = commandTarget as IOleCommandTarget;
            if (oleCommandTarget != null) {
                return new OleToCommandTargetShim(editorView.As<ITextView>(), oleCommandTarget);
            }

            Debug.Fail("Unknown command taget type");
            return null;
        }

        public object TranslateToHostCommandTarget(IEditorView editorView, object commandTarget) {
            var oleToCommandTargetShim = commandTarget as OleToCommandTargetShim;
            if (oleToCommandTargetShim != null) {
                return oleToCommandTargetShim.OleTarget;
            }

            var managedCommandTarget = commandTarget as ICommandTarget;
            if (managedCommandTarget != null) {
                return new CommandTargetToOleShim(editorView.As<ITextView>(), managedCommandTarget);
            }

            var oleCommandTarget = commandTarget as IOleCommandTarget;
            if (oleCommandTarget != null) {
                return oleCommandTarget;
            }

            Debug.Fail("Unknown command taget type");
            return null;
        }

        /// <summary>
        /// Creates compound undo action
        /// </summary>
        /// <param name="editorView">Editor view</param>
        /// <returns>Undo action instance</returns>
        public IEditorUndoAction CreateUndoAction(IEditorView editorView)
            => new VsEditorUndoAction(editorView, _services);
        #endregion
    }
}
