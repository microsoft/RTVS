// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Outline {
    /// <summary>
    /// Generic outlining region
    /// </summary>
    public class OutlineRegion : TextRange {
        protected ITextBuffer _textBuffer;

        public OutlineRegion(ITextBuffer textBuffer, ITextRange range)
            : this(textBuffer, range.Start, range.Length) {
        }

        public OutlineRegion(ITextBuffer textBuffer, int start, int length)
            : base(start, length) {
            _textBuffer = textBuffer;
        }

        public static OutlineRegion FromBounds(ITextBuffer textBuffer, int start, int end) {
            return new OutlineRegion(textBuffer, start, end - start);
        }

        /// <summary>
        /// Text to display in a tooltip when region is collapsed
        /// </summary>
        public virtual string HoverText {
            get {
                if (_textBuffer != null) {
                    int hoverTextLength = Math.Min(this.Length, 512);
                    hoverTextLength = Math.Min(hoverTextLength, _textBuffer.CurrentSnapshot.Length - this.Start);

                    var text = _textBuffer.CurrentSnapshot.GetText(this.Start, hoverTextLength);
                    if (hoverTextLength < this.Length) {
                        text += "...";
                    }

                    return text;
                }

                return String.Empty;
            }
        }

        /// <summary>
        /// Text to display instead of a region when region is collapsed
        /// </summary>
        public virtual string DisplayText => "...";
    }
}
