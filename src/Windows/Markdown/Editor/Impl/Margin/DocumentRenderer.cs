// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Markdig.Renderers;
using Markdig.Syntax;

namespace Microsoft.Markdown.Editor.Margin {
    internal sealed class DocumentRenderer {
        private readonly REvalRenderer _codeEvalRenderer = new REvalRenderer();

        public string Render(MarkdownDocument document) {
            var htmlWriter = new StringWriter();
            var htmlRenderer = new HtmlRenderer(htmlWriter);
            MarkdownFactory.Pipeline.Setup(htmlRenderer);

            htmlRenderer.ObjectRenderers.Insert(0, _codeEvalRenderer);
            htmlRenderer.UseNonAsciiNoEscape = true;
            htmlRenderer.Render(document);
            htmlWriter.Flush();

            var html = htmlWriter.ToString();
            return html;
        }
    }
}
