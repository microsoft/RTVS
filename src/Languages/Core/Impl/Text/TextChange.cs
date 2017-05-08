// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Abstracts text change information allowing code that handles text changes 
    /// to use <seealso cref="ITextProvider"/> rather than Visual Studio ITextBuffer
    ///  or some other platform-specific types.
    /// </summary>
    public class TextChange {
        /// <summary>
        /// Start position of the change
        /// </summary>
        public int Start { get; protected set; }

        /// <summary>
        /// Length of the fragment that was deleted or replaced.
        /// Zero if operation is 'insert' or 'paste' without selection.
        /// </summary>
        public int OldEnd => Start + OldLength;

        /// <summary>
        /// Length of the new fragment. Zero if operation is 'delete'.
        /// </summary>
        public int NewEnd => Start + NewLength;

        /// <summary>
        /// Length of the fragment that was deleted or replaced.
        /// Zero if operation is 'insert' or 'paste' without selection.
        /// </summary>
        public int OldLength { get; protected set; }

        /// <summary>
        /// Length of the new fragment. Zero if operation is 'delete'.
        /// </summary>
        public int NewLength { get; protected set; }

        /// <summary>
        /// Snaphot before the change
        /// </summary>
        public ITextProvider OldTextProvider { get; protected set; }

        /// <summary>
        /// Snapshot after the change
        /// </summary>
        public ITextProvider NewTextProvider { get; protected set; }

        /// <summary>
        ///Text before the change
        /// </summary>
        public string OldText => OldTextProvider?.GetText(OldRange) ?? string.Empty;

        /// <summary>
        /// Snapshot after the change
        /// </summary>
        public string NewText => NewTextProvider?.GetText(NewRange) ?? string.Empty;

        /// <summary>
        /// Changed range in the old snapshot.
        /// </summary>
        public ITextRange OldRange => new TextRange(Start, OldLength);

        /// <summary>
        /// Changed range in the current snapshot.
        /// </summary>
        public ITextRange NewRange => new TextRange(Start, NewLength);

        [DebuggerStepThrough]
        public TextChange(int start, int oldLength, int newLength) : this(start, oldLength, newLength, null, null) { }

        [DebuggerStepThrough]
        public TextChange(int start, int oldLength, int newLength, ITextProvider oldTextProvider, ITextProvider newTextProvider) {
            Check.ArgumentOutOfRange(nameof(start), () => start < 0);
            Check.ArgumentOutOfRange(nameof(start), () => oldLength < 0);
            Check.ArgumentOutOfRange(nameof(start), () => newLength < 0);

            Start = start;
            OldLength = oldLength;
            NewLength = newLength;
            OldTextProvider = oldTextProvider;
            NewTextProvider = newTextProvider;
        }

        public virtual void Clear() {
            Start = OldLength = NewLength = 0;
            OldTextProvider = NewTextProvider = null;
        }
    }
}
