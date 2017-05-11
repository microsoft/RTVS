// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Text provider. Parser calls this interface to retrieve text.
    /// Can be implemented on a string <seealso cref="TextStream"/> or
    /// over Visual Studio ITextBuffer (see Microsoft.R.Editor implementation)
    /// </summary>
    public interface ITextProvider: ITextIterator {
        /// <summary>Retrieves complete text</summary>
        string GetText();
        /// <summary>Retrieves a substring from text range</summary>
        string GetText(ITextRange range);

        /// <summary>Finds first index of a text sequence. Returns -1 if not found.</summary>
        int IndexOf(string text, int startPosition, bool ignoreCase);

        /// <summary>Finds first index of a character. Returns -1 if not found.</summary>
        int IndexOf(char ch, int startPosition);

        /// <summary>Finds first index of a character within given range. Returns -1 if not found.</summary>
        int IndexOf(char ch, ITextRange range);

        /// <summary>Finds first index of a text sequence. Returns -1 if not found.</summary>
        int IndexOf(string text, ITextRange range, bool ignoreCase);

        /// <summary>Compares text range to a given string.</summary>
        bool CompareTo(int position, int length, string text, bool ignoreCase);

        /// <summary>Clones text provider and all its data (typically for use in another thread).</summary>
        ITextProvider Clone();

        /// <summary>Snapshot version.</summary>
        int Version { get; }

        /// <summary>Fires when text buffer content changes.</summary>
        event EventHandler<TextChangeEventArgs> OnTextChange;
    }
}
