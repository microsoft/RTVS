// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Linq;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Extensions;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    internal sealed class RLanguageHandler : ContainedLanguageHandler {
        private readonly RCodeSeparatorCollection _separators = new RCodeSeparatorCollection();
        private readonly IBufferGenerator _generator = new BufferGenerator();
        private readonly IProjectionBufferManager _projectionBufferManager;
        private readonly ICoreShell _coreShell;

        public RLanguageHandler(ITextBuffer textBuffer, IProjectionBufferManager projectionBufferManager, ICoreShell coreShell) :
            base(textBuffer, coreShell) {
            _projectionBufferManager = projectionBufferManager;
            _coreShell = coreShell;
            UpdateProjections();
        }

        public override ICommandTarget GetCommandTargetOfLocation(ITextView textView, int bufferPosition) {
            var block = GetLanguageBlockOfLocation(bufferPosition);
            return block != null ? (ICommandTarget)RMainController.FromTextView(textView) : null;
        }

        public override ITextRange GetCodeBlockOfLocation(int bufferPosition) {
            return GetLanguageBlockOfLocation(bufferPosition);
        }

        protected override void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            var changes = e.ConvertToRelative();
            foreach (var c in changes) {
                var destructive = _separators.IsDestructiveChange(c.OldStart, c.OldLength, c.NewLength, c.OldText, c.NewText);
                if (destructive) {
                    // Allow existing command call to complete so we don't yank projections
                    // from underneath code that expects text buffer to exist, such as formatter.
                    IdleTimeAction.Cancel(GetType());
                    IdleTimeAction.Create(UpdateProjections, 0, GetType(), _coreShell);
                    break;
                } else {
                    Blocks.ReflectTextChange(c.OldStart, c.OldLength, c.NewLength);
                    _separators.ReflectTextChange(c.OldStart, c.OldLength, c.NewLength);
                }
            }
        }

        private void UpdateProjections() {
            ProjectionMapping[] mappings;

            BuildLanguageBlockCollection();
            var content = _generator.GenerateContent(TextBuffer.CurrentSnapshot, Blocks, out mappings);
            _projectionBufferManager.SetProjectionMappings(content, mappings);
        }

        private void BuildLanguageBlockCollection() {
            var tokenizer = new MdTokenizer();
            var tokens = tokenizer.Tokenize(TextBuffer.CurrentSnapshot.GetText());

            var rCodeTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.Code);
            // TODO: incremental updates
            Blocks.Clear();
            _separators.Clear();

            foreach (var t in rCodeTokens) {
                // Verify that code block is properly terminated.
                // If not, it ends at the end of the buffer.
                var text = t.GetText(TextBuffer.CurrentSnapshot);
                var leadingBackticks = LeadingBackTickCount(text);
                var trailingBackticks = TrailingBackTickCount(text);

                _separators.Add(new TextRange(t.Start, leadingBackticks > 1 ? leadingBackticks + 2 : leadingBackticks + 1)); // ```r{ or `r
                if (t.IsWellFormed) {
                    // Count backticks
                    Blocks.Add(TextRange.FromBounds(t.Start + leadingBackticks, t.End - trailingBackticks));
                    _separators.Add(new TextRange(t.End - trailingBackticks, trailingBackticks));
                } else {
                    Blocks.Add(TextRange.FromBounds(t.Start + leadingBackticks, t.End));
                }
            }
        }

        private int LeadingBackTickCount(string s) {
            for (int i = 0; i < s.Length; i++) {
                if (s[i] != '`') {
                    return i;
                }
            }
            return s.Length;
        }

        private int TrailingBackTickCount(string s) {
            for (int i = s.Length - 1; i >= 0; i--) {
                if (s[i] != '`') {
                    return s.Length - 1 - i;
                }
            }
            return s.Length;
        }
    }
}
