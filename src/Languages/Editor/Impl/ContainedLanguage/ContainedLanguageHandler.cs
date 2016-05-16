// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Collections.Generic;
using Microsoft.R.Components.Controller;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.ContainedLanguage {
    public abstract class ContainedLanguageHandler: IContainedLanguageHandler {
        protected LanguageBlockCollection Blocks { get; } = new LanguageBlockCollection();
        protected ITextBuffer TextBuffer { get; }

        private int _cachedPosition = -1;
        private LanguageBlock _cachedLanguageBlock;

        public ContainedLanguageHandler(ITextBuffer textBuffer) {
            TextBuffer = textBuffer;
            TextBuffer.Changed += OnTextBufferChanged;
        }

        public ICommandTarget GetCommandTargetOfLocation(ITextView textView, int bufferPosition) {
            return GetLanguageBlockOfLocation(bufferPosition)?.GetCommandTarget(textView);
        }

        public IContentType GetContentTypeOfLocation(int bufferPosition) {
            return GetLanguageBlockOfLocation(bufferPosition)?.ContentType;
        }

        protected abstract void OnTextBufferChanged(object sender, TextContentChangedEventArgs e);

        private LanguageBlock GetLanguageBlockOfLocation(int bufferPosition) {
            if (_cachedPosition != bufferPosition) {
                _cachedLanguageBlock = Blocks.GetAtPosition(bufferPosition);
                _cachedPosition = bufferPosition;
            }
            return _cachedLanguageBlock;
        }
    }
}
