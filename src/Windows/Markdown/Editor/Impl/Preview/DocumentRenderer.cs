// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using mshtml;
using Markdig.Renderers;
using Markdig.Syntax;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Markdown.Editor.Preview {
    /// <summary>
    /// Renders R Markdown document into HTML
    /// </summary>
    internal sealed class DocumentRenderer : IDisposable {
        private readonly RCodeBlockRenderer _codeBlockRenderer;

        public DocumentRenderer(string documentName, IServiceContainer services) {
            _codeBlockRenderer = new RCodeBlockRenderer(documentName, services);
        }

        public string RenderStaticHtml(MarkdownDocument document) {
            var htmlWriter = new StringWriter();
            var htmlRenderer = new HtmlRenderer(htmlWriter);
            MarkdownFactory.Pipeline.Setup(htmlRenderer);

            using (_codeBlockRenderer.StartRendering()) {
                htmlRenderer.ObjectRenderers.Insert(0, _codeBlockRenderer);

                htmlRenderer.UseNonAsciiNoEscape = true;
                htmlRenderer.Render(document);
                htmlWriter.Flush();

                var html = htmlWriter.ToString();
                return html;
            }
        }

        public Task RenderCodeBlocks(HTMLDocument htmlDocument)
            => _codeBlockRenderer.RenderBlocksAsync(htmlDocument);

        public Task RenderCodeBlocks(HTMLDocument htmlDocument, int start, int count)
            => _codeBlockRenderer.RenderBlocksAsync(htmlDocument, start, count, CancellationToken.None);

        public void Dispose() => _codeBlockRenderer.Dispose();
    }
}
