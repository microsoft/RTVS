// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.Snippets;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// R language code snippets completion provider.
    /// </summary>
    public class SnippetCompletionProvider : IRCompletionListProvider {
        private readonly ISnippetInformationSourceProvider _snippetInformationSource;
        private readonly IImageService _imageService;
        private readonly object _snippetGlyph;

        public SnippetCompletionProvider(IServiceContainer serviceContainer) {
            _snippetInformationSource = serviceContainer.GetService<ISnippetInformationSourceProvider>();
            _imageService = serviceContainer.GetService<IImageService>();
            _snippetGlyph = _imageService.GetImage(ImageType.Snippet);
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<ICompletionEntry> GetEntries(IRIntellisenseContext context, string prefixFilter = null) {
            var completions = new List<ICompletionEntry>();
            if (_snippetInformationSource?.InformationSource != null && !context.IsCaretInNamespace()) {
                var snippets = _snippetInformationSource.InformationSource.Snippets;
                completions.AddRange(snippets.Select(info => new EditorCompletionEntry(info.Name, info.Name, info.Description, _snippetGlyph)));
            }
            return completions;
        }
        #endregion
    }
}
