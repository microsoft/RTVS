// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.EditorFactory;
using Microsoft.Languages.Editor.Extensions;
using Microsoft.Languages.Editor.Services;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.Extensions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    /// <summary>
    /// Host for contained (embedded) language editors such as R editor inside 
    /// ``` code ``` blocks in R Markdown.
    /// </summary>
    public class MdContainedLanguageHost : IContainedLanguageHost {
        private readonly ITextBuffer _textBuffer;
        private readonly ICoreShell _coreShell;
        private IEditorDocument _document;

        /// <summary>
        /// Creates contained language host with default settings.
        /// </summary>
        /// <param name="document">Markdown editor document</param>
        /// <param name="textBuffer">Contained language text buffer</param>
        /// <param name="coreShell"></param>
        public MdContainedLanguageHost(IEditorDocument document, ITextBuffer textBuffer, ICoreShell coreShell) {
            _textBuffer = textBuffer;
            _coreShell = coreShell;

            _document = document;
            _document.DocumentClosing += OnDocumentClosing;

            ServiceManager.AddService<IContainedLanguageHost>(this, textBuffer, coreShell);
        }

        private void OnDocumentClosing(object sender, EventArgs e) {
            if (_document != null) {
                Closing?.Invoke(this, EventArgs.Empty);
                _document.DocumentClosing -= OnDocumentClosing;
            }
        }

        public ICommandTarget ContainedCommandTarget { get; private set; }

        #region IContainedLanguageHost
        public event EventHandler<EventArgs> Closing;

        public string DocumentPath {
            get { return _document != null ? _document.TextBuffer.GetTextDocument().FilePath : string.Empty; }
        }

        public ICommandTarget SetContainedCommandTarget(ITextView textView, ICommandTarget containedCommandTarget) { 
            ContainedCommandTarget = containedCommandTarget;
            return GetBaseCommandTarget(textView);
        }

        public void RemoveContainedCommandTarget(ITextView textView) {
            ContainedCommandTarget = null;
        }

        public bool CanFormatLine(ITextView textView, ITextBuffer containedLanguageBuffer, int lineNumber) {
            var line = containedLanguageBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
            var viewPoint = textView.MapUpToView(line.Start);
            if (viewPoint.HasValue) {
                var lineText = textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(viewPoint.Value).GetText();
                return lineText.IndexOfOrdinal("```") < 0 && !lineText.TrimStart().StartsWithIgnoreCase("```{r");
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Retrieves base command target that is chained to the main controller
        /// attached to a given view. This is typically a core editor command target.
        /// </summary>
        /// <param name="textView">Text view</param>
        /// <returns>Chained command target</returns>
        private ICommandTarget GetBaseCommandTarget(ITextView textView) {
            var controller = MdMainController.FromTextView(textView);

            // If there is no chained target yet, create a simple proxy target
            // for now. Real target will be set later.
            if (controller != null && controller.ChainedController != null) {
                return controller.ChainedController;
            }
            return CommandTargetProxy.GetProxyTarget(textView, _coreShell);
        }
    }
}
