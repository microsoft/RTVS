// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Markdown.Editor.Margin {
    public sealed class StylesheetUpdatedEventArgs: EventArgs {
        public string FileName { get; }

        public StylesheetUpdatedEventArgs(string fileName) {
            FileName = fileName;
        }
    }
}
