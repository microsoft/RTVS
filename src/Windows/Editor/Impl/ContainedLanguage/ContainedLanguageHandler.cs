// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    public abstract class ContainedLanguageHandler: IContainedLanguageHandler {
        protected TextRangeCollection<ITextRange> Blocks { get; } = new TextRangeCollection<ITextRange>();
        protected ITextBuffer TextBuffer { get; }

        private int _cachedPosition = -1;
        private ITextRange _cachedLanguageBlock;

        protected ContainedLanguageHandler(ITextBuffer textBuffer) {
            textBuffer.AddService(this);
            TextBuffer = textBuffer;
            TextBuffer.Changed += OnTextBufferChanged;
        }

        protected abstract void OnTextBufferChanged(object sender, TextContentChangedEventArgs e);

        #region IContainedLanguageHandler

        /// <summary>
        /// Collection of code blocks
        /// </summary>
        public IReadOnlyTextRangeCollection<ITextRange> LanguageBlocks => Blocks;

        /// <summary>
        /// Retrieves contained command target for a given location in the buffer.
        /// </summary>
        /// <param name="textView">Text view</param>
        /// <param name="bufferPosition">Position in the document buffer</param>
        /// <returns>Command target or null if location appears to be primary</returns>
        public abstract ICommandTarget GetCommandTargetOfLocation(ITextView textView, int bufferPosition);
        /// <summary>
        /// Locates contained language block for a given location.
        /// </summary>
        /// <returns>block range or null if no secondary block found</returns>
        public abstract ITextRange GetCodeBlockOfLocation(int bufferPosition);
        #endregion

        protected ITextRange GetLanguageBlockOfLocation(int bufferPosition) {
            if (_cachedPosition != bufferPosition) {
                var items = Blocks.GetItemsContainingInclusiveEnd(bufferPosition);
                int index;
                if (items.Count > 0) {
                    index = items[0];
                } else { 
                    index = Blocks.GetItemAtPosition(bufferPosition);
                }
                _cachedLanguageBlock = index >= 0 ? Blocks[index] : null;
                _cachedPosition = bufferPosition;
            }
            return _cachedLanguageBlock;
        }
    }
}
