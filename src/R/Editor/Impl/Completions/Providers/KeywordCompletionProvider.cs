// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Snippets;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// R language keyword completion provider.
    /// </summary>
    public class KeywordCompletionProvider : IRCompletionListProvider, IRHelpSearchTermProvider {
        private readonly ISnippetInformationSourceProvider _snippetInformationSource;
        private readonly IImageService _imageService;

        public KeywordCompletionProvider(IServiceContainer serviceContainer) {
            _snippetInformationSource = serviceContainer.GetService<ISnippetInformationSourceProvider>();
            _imageService = serviceContainer.GetService<IImageService>();
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<ICompletionEntry> GetEntries(IRIntellisenseContext context, string prefixFilter = null) {
            var completions = new List<ICompletionEntry>();
            if (!context.IsCaretInNamespace()) {
                var infoSource = _snippetInformationSource?.InformationSource;
                var keyWordGlyph = _imageService.GetImage(ImageType.Keyword);

                // Union with constants like TRUE and other common things
                var keywords = Keywords.KeywordList.Concat(Logicals.LogicalsList).Concat(Constants.ConstantsList);
                foreach (var keyword in keywords) {
                    var isSnippet = infoSource?.IsSnippet(keyword);
                    if (!isSnippet.HasValue || !isSnippet.Value) {
                        completions.Add(new EditorCompletionEntry(keyword, keyword, string.Empty, keyWordGlyph));
                    }
                }

                var buildInGlyph = _imageService.GetImage(ImageType.Keyword);
                foreach (var s in Builtins.BuiltinList) {
                    completions.Add(new EditorCompletionEntry(s, s, string.Empty, buildInGlyph));
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
