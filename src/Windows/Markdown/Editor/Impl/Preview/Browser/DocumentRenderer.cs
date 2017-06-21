// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Markdig.Renderers;
using Markdig.Syntax;
using Microsoft.Common.Core.Services;
using Microsoft.Markdown.Editor.Preview.Code;
using Microsoft.Markdown.Editor.Preview.Parser;
using mshtml;

namespace Microsoft.Markdown.Editor.Preview.Browser {
    /// <summary>
    /// Renders R Markdown document into HTML
    /// </summary>
    internal sealed class DocumentRenderer : IDisposable {
        private readonly RCodeBlockRenderer _codeBlockRenderer;
        private readonly YamlRenderer _yamlRenderer;

        public DocumentRenderer(string documentName, IServiceContainer services) {
            _codeBlockRenderer = new RCodeBlockRenderer(documentName, services);
            _yamlRenderer = new YamlRenderer();
        }

        public string RenderStaticHtml(MarkdownDocument document) {
            var htmlWriter = new StringWriter();
            var htmlRenderer = new HtmlRenderer(htmlWriter);
            MarkdownFactory.Pipeline.Setup(htmlRenderer);

            using (_codeBlockRenderer.StartRendering()) {
                htmlRenderer.ObjectRenderers.Insert(0, _codeBlockRenderer);
                htmlRenderer.ObjectRenderers.Insert(0, _yamlRenderer);

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
