// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using static System.FormattableString;

namespace Microsoft.Markdown.Editor.Margin {
    internal sealed class REvalRenderer : HtmlObjectRenderer<CodeBlock>, IDisposable {
        private readonly string _sessionId;
        private readonly Dictionary<string, string> _renderedResults = new Dictionary<string, string>();
        private readonly IServiceContainer _services;
        private readonly IRInteractiveWorkflow _workflow;
        private IRSession _session;

        public REvalRenderer(string name, IServiceContainer services) {
            _services = services;
            _workflow = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
            _sessionId = Invariant($"({name} - {Guid.NewGuid()}");
        }

        protected override void Write(HtmlRenderer renderer, CodeBlock obj) {
            renderer.EnsureLine();

            var codeBlock = obj as FencedCodeBlock;
            if (codeBlock?.Info != null && codeBlock.Info.StartsWith()) {
                var id = GetBlockId(codeBlock);

                if (_renderedResults.TryGetValue(id, out string result)) {
                    renderer.Write(result);
                    return;
                }

                var session = GetSession();
                var code = codeBlock.ToPositionText();


                // We are replacing the HTML attribute `language-mylang` by `mylang` only for a div block
                // NOTE that we are allocating a closure here
                renderer.Write("<img src='https://www.r-project.org/Rlogo.png' />");

            }

        }

        private string GetBlockId(FencedCodeBlock block) => Invariant($"{block.Line} {block.ToPositionText()}");

        private IRSession GetSession() {
            _session = _session ??_workflow.RSessions.GetOrCreate(_sessionId);
            return _session;
        }

        public void Dispose() {
            _session?.Dispose();
        }
    }
}
