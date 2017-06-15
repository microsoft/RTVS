// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Markdig.Syntax;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Markdown.Editor.Preview {
    public sealed class ParsingEventArgs : EventArgs {
        public ParsingEventArgs(MarkdownDocument document, string file, ITextSnapshot snapshot) {
            Document = document;
            File = file;
            Snapshot = snapshot;
        }

        public MarkdownDocument Document { get; }
        public string File { get; }
        public ITextSnapshot Snapshot { get; }
    }
}
