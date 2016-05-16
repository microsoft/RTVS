// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Languages.Editor.ContainedLanguage;
using Microsoft.Languages.Editor.Projection;
using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Markdown.Editor.ContainedLanguage {
    internal sealed class RLanguageHandler : ContainedLanguageHandler {
        private readonly RCodeFragmentCollection _fragments = new RCodeFragmentCollection();
        private readonly IProjectionBufferManager _pbm;

        public RLanguageHandler(ITextBuffer textBuffer, IProjectionBufferManager pbm) : 
            base(textBuffer) {
            _pbm = pbm;
        }

        protected override void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            var changes = e.ConvertToRelative();
            foreach (var c in changes) {
                _fragments.IsDestructiveChange(c.OldStart, c.OldLength, c.NewLength, c.OldText, c.NewText);
            }
        }
    }
}
