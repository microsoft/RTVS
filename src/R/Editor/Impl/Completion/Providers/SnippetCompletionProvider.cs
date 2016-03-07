// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Editor.Snippets;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// R language code snippets completion provider.
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public class SnippetCompletionProvider : IRCompletionListProvider {
        [Import(AllowDefault = true)]
        private ISnippetInformationSourceProvider SnippetInformationSource { get; set; }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            var infoSource = SnippetInformationSource?.InformationSource;

            if (!context.IsInNameSpace() && infoSource != null) {
                ImageSource snippetGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphCSharpExpansion, StandardGlyphItem.GlyphItemPublic);
                foreach (string name in infoSource.SnippetNames) {
                    completions.Add(new RCompletion(name, name, string.Empty, snippetGlyph));
                }
            }
            return completions;
        }
        #endregion
    }
}
