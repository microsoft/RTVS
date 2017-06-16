// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor {
    public static class TextViewExtensions {
        public static ITextRange GetCurrentRCodeBlock(this ITextView textView)
            => textView.GetRCodeBlock(textView.Caret.Position.BufferPosition);

        public static ITextRange GetRCodeBlock(this ITextView textView, int position) {
            var containedLanguageHandler = textView.GetContainerLanguageHandler();
            return containedLanguageHandler?.GetCodeBlockOfLocation(position);
        }

        public static int? GetRCodeBlockNumber(this ITextView textView, int position) {
            var containedLanguageHandler = textView.GetContainerLanguageHandler();
            return containedLanguageHandler?.LanguageBlocks.GetItemContaining(position);
        }

        public static int? GetCurrentRCodeBlockNumber(this ITextView textView)
            => textView.GetRCodeBlockNumber(textView.Caret.Position.BufferPosition);

        public static bool IsCaretInRCode(this ITextView textView)
        => textView.IsPositionInRCode(textView.Caret.Position.BufferPosition);

        public static bool IsPositionInRCode(this ITextView textView, int position) => textView.GetRCodeBlock(position) != null;

        public static IContainedLanguageHandler GetContainerLanguageHandler(this ITextView textView) {
            var rmdBuffer = textView.BufferGraph.GetTextBuffers(b => b.ContentType.DisplayName.EqualsOrdinal(MdContentTypeDefinition.ContentType)).First();
            return rmdBuffer?.GetService<IContainedLanguageHandler>();
        }
    }
}
