// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Common.Core.Imaging;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.Snippets;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// R language code snippets completion provider.
    /// </summary>
    public class SnippetCompletionProvider : IRCompletionListProvider {
        private readonly ISnippetInformationSourceProvider _snippetInformationSource;
        private readonly object _snippetGlyph;

        public SnippetCompletionProvider(ISnippetInformationSourceProvider snippetInformationSource, IImageService imageService) {
            _snippetInformationSource = snippetInformationSource;
            _snippetGlyph = imageService.GetImage(ImageType.Snippet);
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<ICompletionEntry> GetEntries(IRCompletionContext context) {
            var completions = new List<ICompletionEntry>();
            var infoSource = _snippetInformationSource?.InformationSource;

            if (!context.IsCaretInNameSpace() && infoSource != null) {
                foreach (ISnippetInfo info in infoSource.Snippets) {
                    completions.Add(new EditorCompletionEntry(info.Name, info.Name, info.Description, _snippetGlyph));
                }
            }
            return completions;
        }
        #endregion
    }
}
