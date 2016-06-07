// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Linq;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.ContainedLanguage;
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

        public RLanguageHandler(ITextBuffer textBuffer, IProjectionBufferManager projectionBufferManager) :
            base(textBuffer) {
            _projectionBufferManager = projectionBufferManager;
            UpdateProjections();
        }

        public override ICommandTarget GetCommandTargetOfLocation(ITextView textView, int bufferPosition) {
            var block = GetLanguageBlockOfLocation(bufferPosition);
            return block != null ? (ICommandTarget)RMainController.FromTextView(textView) : null;
        }

        public override ITextRange GetCodeBlockOfLocation(ITextView textView, int bufferPosition) {
            return GetLanguageBlockOfLocation(bufferPosition);
        }

        protected override void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            var changes = e.ConvertToRelative();
            foreach (var c in changes) {
                var destructive = _separators.IsDestructiveChange(c.OldStart, c.OldLength, c.NewLength, c.OldText, c.NewText);
                if (destructive) {
                    // Allow existing command call to complete so we don't yank projections
                    // from underneath code that expects text buffer to exist, such as formatter.
                    IdleTimeAction.Cancel(this.GetType());
                    IdleTimeAction.Create(() => UpdateProjections(), 0, this.GetType());
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
                Blocks.Add(new TextRange(t.Start + 3, t.Length - 6));
                _separators.Add(new TextRange(t.Start, 5));
                _separators.Add(new TextRange(t.End - 3, 3));
            }
        }
    }
}
