// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Core.Text {
    public class TextEdit {
        public ITextRange Range { get; }
        public string NewText { get; }

        public TextEdit(ITextRange range, string newText) {
            Range = range;
            NewText = newText;
        }
    }
}
