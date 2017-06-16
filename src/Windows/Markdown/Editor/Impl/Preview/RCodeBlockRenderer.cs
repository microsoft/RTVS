// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using mshtml;
using Microsoft.Languages.Core.Text;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.R.Host.Client.Host;
using static System.FormattableString;

namespace Microsoft.Markdown.Editor.Preview {
    /// <summary>
    /// Renders R code block output into HTML element
    /// </summary>
    internal sealed class RCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>, IDisposable {
        private static string _dynamicPlaceholderImage;

        /// <summary>
        /// Rendered results cache. Caches HTML for every code block.
        /// Blocks are differentiated by their order of appearance
        /// and the contents hash.
        /// </summary>
        private readonly List<RCodeBlock> _blocks = new List<RCodeBlock>();

        private readonly IServiceContainer _services;
        private readonly IRMarkdownEditorSettings _settings;
        private readonly string _sessionId;
        private readonly CancellationTokenSource _hostStartCts = new CancellationTokenSource();
        private readonly RSessionCallback _sessionCallback = new RSessionCallback();

        private CancellationTokenSource _blockEvalCts = new CancellationTokenSource();
        private IRSession _session;
        private int _blockNumber;
        private Task _evalTask = Task.CompletedTask;

        public RCodeBlockRenderer(string documentName, IServiceContainer services) {
            _services = services;
            _settings = services.GetService<IRMarkdownEditorSettings>();
            _sessionId = Invariant($"({documentName} - {Guid.NewGuid()}");
            StartSessionAsync(_hostStartCts.Token).DoNotWait();
        }

        public IDisposable StartRendering() {
            _blockNumber = 0;
            _blockEvalCts?.Cancel();
            _blockEvalCts = new CancellationTokenSource();
            return Disposable.Create(() => _evalTask = _settings.AutomaticSync ? EvaluateBlocksAsync(_blockEvalCts.Token) : Task.CompletedTask);
        }

        #region Evaluation
        private Task EvaluateBlockAsync(IRSession session, int blockNumber, CancellationToken ct) {
            var block = _blocks[blockNumber];
            block.State = CodeBlockEvalState.Created;
            return block.EvaluateAsync(session, _sessionCallback, ct);
        }


        private Task EvaluateBlocksAsync(CancellationToken ct) {
            var blocks = _blocks.ToArray();
            //TODO: clear session on cache drop
            return Task.Run(async () => {
                try {
                    var session = await StartSessionAsync(ct);

                    foreach (var block in blocks.Where(b => b.State == CodeBlockEvalState.Created)) {
                        await block.EvaluateAsync(session, _sessionCallback, ct);
                    }
                } catch (OperationCanceledException) { }
            }, ct);
        }
        #endregion

        #region Rendering
        public async Task RenderBlocksAsync(HTMLDocument htmlDocument) {
            await _evalTask;
            var blocks = _blocks.ToArray();
            foreach (var b in blocks.Where(b => b.State == CodeBlockEvalState.Evaluated)) {
                RenderBlock(htmlDocument, b.BlockNumber);
            }
        }

        public async Task RenderBlocksAsync(HTMLDocument htmlDocument, int start, int count, CancellationToken ct) {
            await _evalTask;
            try {
                var session = await StartSessionAsync(ct);
                for (var i = start; i < start + count; i++) {
                    await EvaluateBlockAsync(session, i, ct);
                    RenderBlock(htmlDocument, i);
                }
            } catch (OperationCanceledException) { }
        }

        private void RenderBlock(HTMLDocument htmlDocument, int blockNumber) {
            var block = _blocks[blockNumber];
            var element = htmlDocument.getElementById(block.HtmlElementId);
            if (element != null) {
                element.innerHTML = block.Result;
                block.State = CodeBlockEvalState.Rendered;
            }
        }
        #endregion

        #region Writing
        protected override void Write(HtmlRenderer renderer, CodeBlock codeBlock) {
            renderer.EnsureLine();

            var fencedCodeBlock = codeBlock as FencedCodeBlock;
            if (fencedCodeBlock?.Info != null && fencedCodeBlock.Info.StartsWithIgnoreCase("{r")) {
                var text = GetBlockText(fencedCodeBlock);
                var hash = text.GetHashCode();

                var result = GetCachedResult(_blockNumber, hash, fencedCodeBlock);
                if (result != null) {
                    WriteBlockContent(renderer, _blockNumber, text);
                    renderer.Write(result);
                } else {
                    var rCodeBlock = new RCodeBlock(_blockNumber, fencedCodeBlock.Arguments, text, hash);
                    var elementId = rCodeBlock.HtmlElementId;
                    _blocks.Add(rCodeBlock);

                    var echoed = WriteBlockContent(renderer, _blockNumber, text);
                    // Avoid duplicate code in non-automatic case
                    if (!echoed || _settings.AutomaticSync) {
                        // Write placeholder first. We will insert actual data when the evaluation is done.
                        renderer.Write(GetBlockPlaceholder(elementId, text));
                    }
                }
                _blockNumber++;
            }
        }

        private bool WriteBlockContent(HtmlRenderer renderer, int blockNumber, string text) {
            if (_blocks[blockNumber].EchoContent) {
                renderer.Write(Invariant($"<pre class='r'><code>{text}</code></pre>"));
                return true;
            }
            return false;
        }
        private static string GetBlockText(FencedCodeBlock block) {
            var sb = new StringBuilder();
            foreach (var line in block.Lines.Lines) {
                sb.AppendLine(line.ToString());
            }
            return sb.ToString().Trim();
        }
        #endregion

        #region Session
        private async Task<IRSession> StartSessionAsync(CancellationToken ct) {
            if (_session == null) {
                var workflow = _services.GetService<IRInteractiveWorkflowProvider>().GetOrCreate();
                _session = workflow.RSessions.GetOrCreate(_sessionId);
            }
            if (!_session.IsHostRunning) {
                var settings = _services.GetService<IRSettings>();
                await _session.EnsureHostStartedAsync(
                    new RHostStartupInfo(settings.CranMirror, codePage: settings.RCodePage), _sessionCallback, 3000, ct);
            }

            return _session;
        }
        #endregion

        #region Cache
        private string GetCachedResult(int blockNumber, int hash, FencedCodeBlock block) {
            if (blockNumber >= _blocks.Count) {
                return null;
            }
            if (_blocks[_blockNumber].Hash != hash) {
                InvalidateCacheFrom(_blockNumber);
                return null;
            }
            // can be null if block hasn't been rendered yet
            return _blocks[_blockNumber].Result;
        }

        private void InvalidateCacheFrom(int index) {
            if (index < _blocks.Count) {
                _blocks.RemoveRange(index, _blocks.Count - index);
            }
        }
        #endregion

        #region Placeholders
        private string GetBlockPlaceholder(string elementId, string text)
            => _settings.AutomaticSync ? GetDynamicPlaceholder(elementId) : GetStaticPlaceholder(elementId, text);

        /// <summary>
        /// Returns spinner image for automatic sync mode
        /// </summary>
        private static string GetDynamicPlaceholder(string elementId) {
            if (_dynamicPlaceholderImage == null) {
                using (var ms = new MemoryStream()) {
                    Resources.Loading.Save(ms, ImageFormat.Gif);
                    _dynamicPlaceholderImage = Convert.ToBase64String(ms.ToArray());
                }
            }
            return Invariant($"<div id='{elementId}'><img src='data:image/gif;base64, {_dynamicPlaceholderImage}' width='32' height='32' /></div>");
        }

        /// <summary>
        /// Returns static image for manual sync mode
        /// </summary>
        private static string GetStaticPlaceholder(string elementId, string text)
            => Invariant($"<div id='{elementId}'><code>{text}</code></div>");
        #endregion

        public void Dispose() {
            _blockEvalCts.Cancel();
            _hostStartCts.Cancel();
            _session?.Dispose();
        }
    }
}
