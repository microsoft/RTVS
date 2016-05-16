// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System.Linq;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.Languages.Editor.Text;
using Microsoft.Markdown.Editor.Commands;
using Microsoft.Markdown.Editor.Tokens;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Commands;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    internal sealed class RLanguageHandler : ContainedLanguageHandler {
        private readonly RCodeFragmentCollection _fragments = new RCodeFragmentCollection();
        private readonly IBufferGenerator _generator;

        public RLanguageHandler(ITextBuffer textBuffer) :
            base(textBuffer) {
            IdleTimeAction.Create(UpdateProjections, 100, this.GetType());
        }

        public override ICommandTarget GetCommandTargetOfLocation(ITextView textView, int bufferPosition) {
            var block = GetLanguageBlockOfLocation(bufferPosition);
            return block != null
                ? (ICommandTarget)RMainController.FromTextView(textView)
                : MdMainController.FromTextView(textView);
        }

        protected override void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            var changes = e.ConvertToRelative();
            bool destructive = false;
            foreach (var c in changes) {
                destructive |= _fragments.IsDestructiveChange(c.OldStart, c.OldLength, c.NewLength, c.OldText, c.NewText);
                if (destructive) {
                    IdleTimeAction.Cancel(this.GetType());
                    IdleTimeAction.Create(UpdateProjections, 200, this.GetType());
                    break;
                } else {
                    Blocks.ReflectTextChange(c.OldStart, c.OldLength, c.NewLength);
                }
            }
        }

        private void UpdateProjections() {
            BuildLanguageBlockCollection();
            _generator.GenerateContent(TextBuffer, Blocks);
        }

        private void BuildLanguageBlockCollection() {
            var tokenizer = new MdTokenizer();
            var tokens = tokenizer.Tokenize(TextBuffer.CurrentSnapshot.GetText());

            var rCodeTokens = tokens.Where(t => t.TokenType == MarkdownTokenType.Code);
            // TODO: incremental updates
            Blocks.Clear();
            foreach (var t in rCodeTokens) {
                Blocks.Add(new TextRange(t.Start + 3, t.Length - 6));
            }
        }
    }
}
