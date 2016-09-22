// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Snippets;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// R language keyword completion provider.
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    [Export(typeof(IRHelpSearchTermProvider))]
    public class KeywordCompletionProvider : IRCompletionListProvider, IRHelpSearchTermProvider {
        private readonly ISnippetInformationSourceProvider _snippetInformationSource;
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public KeywordCompletionProvider([Import(AllowDefault = true)] ISnippetInformationSourceProvider snippetInformationSource, ICoreShell shell) {
            _snippetInformationSource = snippetInformationSource;
            _shell = shell;
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();

            if (!context.IsInNameSpace()) {
                var infoSource = _snippetInformationSource?.InformationSource;
                ImageSource keyWordGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic, _shell);

                // Union with constants like TRUE and other common things
                var keywords = Keywords.KeywordList.Union(Logicals.LogicalsList).Union(Constants.ConstantsList);
                foreach (string keyword in keywords) {
                    bool? isSnippet = infoSource?.IsSnippet(keyword);
                    if (!isSnippet.HasValue || !isSnippet.Value) {
                        completions.Add(new RCompletion(keyword, keyword, string.Empty, keyWordGlyph));
                    }
                }

                ImageSource buildInGlyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphGroupIntrinsic, StandardGlyphItem.GlyphItemPublic, _shell);
                foreach (string s in Builtins.BuiltinList) {
                    completions.Add(new RCompletion(s, s, string.Empty, buildInGlyph));
                }
            }

            return completions;
        }
        #endregion

        #region IRHelpSearchTermProvider
        public IReadOnlyCollection<string> GetEntries() => Keywords.KeywordList;
        #endregion
    }
}
