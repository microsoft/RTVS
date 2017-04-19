// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Editor.Snippets;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// R language code snippets completion provider.
    /// </summary>
    [Export(typeof(IRCompletionListProvider))]
    public class SnippetCompletionProvider : IRCompletionListProvider {
        private readonly ISnippetInformationSourceProvider _snippetInformationSource;
        private readonly ImageSource _snippetGlyph;

        [ImportingConstructor]
        public SnippetCompletionProvider([Import(AllowDefault = true)] ISnippetInformationSourceProvider snippetInformationSource, ICoreShell coreShell) {
            _snippetInformationSource = snippetInformationSource;
            var imageService = coreShell.GetService<IImageService>();
            _snippetGlyph = imageService.GetImage(ImageType.Snippet) as ImageSource;
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            List<RCompletion> completions = new List<RCompletion>();
            var infoSource = _snippetInformationSource?.InformationSource;

            if (!context.IsInNameSpace() && infoSource != null) {
                foreach (ISnippetInfo info in infoSource.Snippets) {
                    completions.Add(new RCompletion(info.Name, info.Name, info.Description, _snippetGlyph));
                }
            }
            return completions;
        }
        #endregion
    }
}
