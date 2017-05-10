// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Linq;
using Microsoft.Common.Core.Idle;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    internal sealed class RLanguageHandler : ContainedLanguageHandler {
        private readonly RCodeSeparatorCollection _separators = new RCodeSeparatorCollection();
        private readonly IBufferGenerator _generator = new BufferGenerator();
        private readonly IProjectionBufferManager _projectionBufferManager;
        private readonly IIdleTimeService idleTime;

        public RLanguageHandler(ITextBuffer textBuffer, IProjectionBufferManager projectionBufferManager, IServiceContainer services) :
            base(textBuffer) {
            _projectionBufferManager = projectionBufferManager;
            idleTime = services.GetService<IIdleTimeService>();
            UpdateProjections();
        }

        public override ICommandTarget GetCommandTargetOfLocation(ITextView textView, int bufferPosition) {
            var block = GetLanguageBlockOfLocation(bufferPosition);
            return block != null ? RMainController.FromTextView(textView) : null;
        }

        public override ITextRange GetCodeBlockOfLocation(int bufferPosition) => GetLanguageBlockOfLocation(bufferPosition);

        protected override void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            var c = e.ToTextChange();
            var destructive = _separators.IsDestructiveChange(c.Start, c.OldLength, c.NewLength, c.OldTextProvider, c.NewTextProvider);
            if (destructive) {
                // Allow existing command call to complete so we don't yank projections
                // from underneath code that expects text buffer to exist, such as formatter.
                IdleTimeAction.Cancel(GetType());
                IdleTimeAction.Create(UpdateProjections, 0, GetType(), idleTime);
            } else {
                Blocks.ReflectTextChange(c.Start, c.OldLength, c.NewLength);
                _separators.ReflectTextChange(c.Start, c.OldLength, c.NewLength);
            }
        }

        private void UpdateProjections() {
            BuildLanguageBlockCollection();
            var content = _generator.GenerateContent(TextBuffer.CurrentSnapshot, Blocks, out var mappings);
            _projectionBufferManager.SetProjectionMappings(content, mappings);
        }

        private void BuildLanguageBlockCollection() {
            var tokenizer = new MdTokenizer();
            var tokens = tokenizer.Tokenize(TextBuffer.CurrentSnapshot.GetText());
            var rCodeTokens = tokens.OfType<MarkdownCodeToken>().Where(t => t.LeadingSeparatorLength > 1);

            // TODO: incremental updates
            Blocks.Clear();
            _separators.Clear();

            foreach (var t in rCodeTokens) {
                // Verify that code block is properly terminated.
                // If not, it ends at the end of the buffer.
                _separators.Add(new TextRange(t.Start, t.LeadingSeparatorLength)); // ```r{ or `r
                if (t.IsWellFormed) {
                    // Count backticks
                    Blocks.Add(TextRange.FromBounds(t.Start + t.LeadingSeparatorLength, t.End - t.TrailingSeparatorLength));
                    _separators.Add(new TextRange(t.End - t.TrailingSeparatorLength, t.TrailingSeparatorLength));
                } else {
                    Blocks.Add(TextRange.FromBounds(t.Start + t.LeadingSeparatorLength, t.End));
                }
            }
        }
    }
}
