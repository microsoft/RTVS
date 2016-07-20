// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Snippets;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// R language code snippets completion provider.
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public class SnippetCompletionProvider : IRCompletionListProvider {
        private readonly ISnippetInformationSourceProvider _snippetInformationSource;
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public SnippetCompletionProvider([Import(AllowDefault = true)] ISnippetInformationSourceProvider snippetInformationSource, ICoreShell shell) {
            _snippetInformationSource = snippetInformationSource;
            _shell = shell;
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            var infoSource = _snippetInformationSource?.InformationSource;

            if (!context.IsInNameSpace() && infoSource != null) {
                ImageSource snippetGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphCSharpExpansion, StandardGlyphItem.GlyphItemPublic, _shell);
                foreach (ISnippetInfo info in infoSource.Snippets) {
                    completions.Add(new RCompletion(info.Name, info.Name, info.Description, snippetGlyph));
                }
            }
            return completions;
        }
        #endregion
    }
}
