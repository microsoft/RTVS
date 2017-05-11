// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class TextBufferUtility {
        public static void ApplyTextChange(ITextBuffer textBuffer, int start, int oldLength, int newLength, string newText) {
            if (oldLength == 0 && newText.Length > 0) {
                textBuffer.Insert(start, newText);
            } else if (oldLength > 0 && newText.Length > 0) {
                textBuffer.Replace(new Span(start, oldLength), newText);
            } else {
                textBuffer.Delete(new Span(start, oldLength));
            }
        }
    }
}
