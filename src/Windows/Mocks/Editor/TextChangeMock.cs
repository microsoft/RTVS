// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class TextChangeMock : ITextChange, IComparable {
        public TextChangeMock() : this(0, 0, string.Empty, string.Empty) { }

        public TextChangeMock(int start, int oldLength, string newText)
            : this(start, oldLength, string.Empty, newText) { }

        public TextChangeMock(int start, int oldLength, string oldText, string newText) {
            NewPosition = start;
            OldLength = oldLength;
            OldText = oldText;
            NewText = newText;
        }

        #region ITextChange Members

        public int Delta => NewText.Length - OldLength;
        public int LineCountDelta => 0;
        public int NewEnd => NewPosition + NewText.Length;
        public int NewLength => NewText.Length;
        public int NewPosition { get; }
        public Span NewSpan => new Span(NewPosition, NewLength);
        public string NewText { get; }
        public int OldEnd => NewPosition + OldLength;
        public int OldLength { get; }
        public int OldPosition => NewPosition;
        public Span OldSpan => new Span(NewPosition, OldLength);
        public string OldText { get; }

        #endregion

        #region IComparable Members
        public int CompareTo(object obj) {
            var other = obj as TextChangeMock;
            return this.OldPosition.CompareTo(other.OldPosition);
        }
        #endregion
    }

}
