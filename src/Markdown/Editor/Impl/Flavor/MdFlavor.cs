using System;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Markdown.Editor.Flavor {
    public static class MdFlavor {
        public static MarkdownFlavor FromTextBuffer(ITextBuffer textBuffer) {
            MarkdownFlavor flavor = MarkdownFlavor.Basic;
            if (textBuffer.Properties.TryGetProperty("MarkdownFlavor", out flavor)) {
                return flavor;
            }

            ITextDocument textDocument = null;
            if (textBuffer.Properties.TryGetProperty(typeof(ITextDocument), out textDocument)) {
                if (!string.IsNullOrEmpty(textDocument.FilePath)) {
                    string ext = Path.GetExtension(textDocument.FilePath);
                    if (ext.EqualsIgnoreCase(".rmd")) {
                        return MarkdownFlavor.R;
                    }
                }
            }

            return MarkdownFlavor.Basic;
        }
    }
}

