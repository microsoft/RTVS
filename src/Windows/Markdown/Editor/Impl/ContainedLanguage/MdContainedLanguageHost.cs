// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Document;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    /// <summary>
    /// Host for contained (embedded) language editors such as R editor inside 
    /// ``` code ``` blocks in R Markdown.
    /// </summary>
    internal sealed class MdContainedLanguageHost : IContainedLanguageHost {
        private readonly ITextBuffer _textBuffer;
        private readonly IEditorDocument _document;

        /// <summary>
        /// Creates contained language host with default settings.
        /// </summary>
        /// <param name="document">Markdown editor document</param>
        /// <param name="editorBuffer">Contained language text buffer</param>
        public MdContainedLanguageHost(IEditorDocument document, IEditorBuffer editorBuffer) {
            _textBuffer = editorBuffer.As<ITextBuffer>();

            _document = document;
            _document.Closing += OnDocumentClosing;

            editorBuffer.AddService(this);
        }

        private void OnDocumentClosing(object sender, EventArgs e) {
            if (_document != null) {
                Closing?.Invoke(this, EventArgs.Empty);
                _document.Closing -= OnDocumentClosing;
            }
        }

        public ICommandTarget ContainedCommandTarget { get; private set; }

        #region IContainedLanguageHost
        /// <summary>
        /// Fires when primary document is closing. After this event certain properties 
        /// like BufferGraph become unavailable and may return null.
        /// </summary>
        public event EventHandler<EventArgs> Closing;

        /// <summary>
        /// Full path to the primary document. Typically used by the contained
        /// language syntax check to output correct path in the task list.
        /// </summary>
        public string DocumentPath => _document?.FilePath ?? string.Empty;

        /// <summary>
        /// Sets command target of the contained language editor.
        /// </summary>
        /// <returns>Command target for the contained language to use as a base</returns>
        public ICommandTarget SetContainedCommandTarget(IEditorView editorView, ICommandTarget containedCommandTarget) {
            ContainedCommandTarget = containedCommandTarget;
            return GetBaseCommandTarget(editorView.As<ITextView>());
        }

        /// <summary>
        /// Removes contained command target
        /// </summary>
        /// <param name="editorView">Text view associated with the command target to remove.</param>
        public void RemoveContainedCommandTarget(IEditorView editorView) => ContainedCommandTarget = null;

        /// <summary>
        /// Determines if secondary language can format given line.
        /// </summary>
        /// <param name="editorView">Text view</param>
        /// <param name="containedLanguageBuffer">Contained language buffer</param>
        /// <param name="lineNumber">Line number in the contained language buffer</param>
        public bool CanFormatLine(IEditorView editorView, IEditorBuffer containedLanguageBuffer, int lineNumber) {
            var textView = editorView.As<ITextView>();
            var textBuffer = containedLanguageBuffer.As<ITextBuffer>();
            var line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(lineNumber);
            var viewPoint = textView.MapUpToView(line.Start);
            if (viewPoint.HasValue) {
                var lineText = textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(viewPoint.Value).GetText();
                return lineText.IndexOfOrdinal("```") < 0 && !lineText.TrimStart().StartsWithIgnoreCase("```{r");
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Detemines if particular range should not be treated as contained language and 
        /// instead should be ignored or 'skipped over'. Used by R parser to ignore 'R' in
        /// ```{R ... }
        /// </summary>
        public bool IsInertRange(ITextRange range) {
            // This is a workaround for constructs like ```{r x = 1, y = FALSE} where the { }
            // block is treated as R fragment. The fragment is syntactually incorrect since
            // 'r' is indentifier and there is an operator expected between 'r' and 'x'.
            // In order to avoid parsing errors expression parser will use this flag and
            // allow standalone indentifier 'r' or 'R' right after the opening curly brace.
            if (range.Length == 1 && range.Start > 0) {
                // Map range up from contained language
                var view = _textBuffer.GetFirstView();
                var viewPoint = view?.MapUpToView(new SnapshotPoint(_textBuffer.CurrentSnapshot, range.Start));
                if (viewPoint.HasValue) {
                    var snapshot = view.TextBuffer.CurrentSnapshot;
                    if (snapshot.Length >= 2 && snapshot.GetText(viewPoint.Value - 1, 2).EqualsIgnoreCase("`r")) {
                        return true;
                    }
                    if (snapshot.Length >= 5 && viewPoint.Value > 3 && snapshot.GetText(viewPoint.Value - 4, 5).EqualsIgnoreCase("```{r")) {
                        return true;
                    }
                }
            }
            return false;
        }

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
            return controller?.ChainedController ?? CommandTargetProxy.GetProxyTarget(textView);
        }
    }
}
