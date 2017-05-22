// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Markdown.Editor.Flavor {
    public static class MdFlavor {
        public static MarkdownFlavor FromTextBuffer(ITextBuffer textBuffer) {
            if (textBuffer.ContentType.TypeName.EqualsOrdinal(MdProjectionContentTypeDefinition.ContentType)) {
                return MarkdownFlavor.R;
            }

            var textDocument = textBuffer.GetTextDocument();
            if (!string.IsNullOrEmpty(textDocument?.FilePath)) {
                string ext = Path.GetExtension(textDocument.FilePath);
                if (ext.EqualsIgnoreCase(".rmd")) {
                    return MarkdownFlavor.R;
                }
            }

            return MarkdownFlavor.Basic;
        }
    }
}

