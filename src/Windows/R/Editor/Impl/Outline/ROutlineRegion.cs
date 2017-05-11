// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Outline;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Outline {
    /// <summary>
    /// Collapsible region in R code
    /// </summary>
    internal class ROutlineRegion : OutlineRegion {
        private string _displayText;

        public ROutlineRegion(ITextBuffer textBuffer, ITextRange range)
            : base(textBuffer, range) {
        }

        public ROutlineRegion(ITextBuffer textBuffer, ITextRange range, string displayText)
            : base(textBuffer, range) {
            _displayText = displayText;
        }

        public override string DisplayText {
            get {
                if (_displayText == null) {
                    _displayText = _textBuffer.CurrentSnapshot.GetText(this.Start, this.Length);
                    int index = _displayText.IndexOfAny(new char[] { '(', '{' });
                    if (index >= 0) {
                        _displayText = _displayText.Substring(0, index).Trim() + "...";
                    } else if (_displayText.Length > 50) {
                        _displayText = _displayText.Substring(0, 50) + "...";
                    }
                }

                return _displayText;
            }
        }
    }
}
