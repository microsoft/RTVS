// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Shell {
    /// <summary>
    /// Host for Web editing component. This interface provides 
    /// application-specific services and settings.
    /// </summary>
    public interface IApplicationEditorSupport {
        /// <summary>
        /// Provides shim that implements ICommandTarget over 
        /// application-specific command target. For example, 
        /// Visual Studio is using IOleCommandTarget.
        /// </summary>
        /// <param name="commandTarget">Command target</param>
        /// <returns>Web components compatible command target</returns>
        ICommandTarget TranslateCommandTarget(ITextView textView, object commandTarget);

        /// <summary>
        /// Provides application-specific command target.
        /// For example, Visual Studio is using IOleCommandTarget.
        /// </summary>
        /// <param name="commandTarget">Command target</param>
        /// <returns>Host compatible command target</returns>
        object TranslateToHostCommandTarget(ITextView textView, object commandTarget);

        /// <summary>
        /// Creates compound undo action
        /// </summary>
        /// <param name="textView">Text view</param>
        /// <param name="textBuffer">Text buffer</param>
        /// <returns>Undo action instance</returns>
        ICompoundUndoAction CreateCompoundAction(ITextView textView, ITextBuffer textBuffer);
    }
}
