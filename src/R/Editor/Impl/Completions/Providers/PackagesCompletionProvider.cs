// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.Imaging;
using Microsoft.Languages.Editor.Completions;
using Microsoft.R.Editor.Functions;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// Provides list of installed packages for completion inside 
    /// library(...) statement. List of packages is  obtained from 
    /// ~\Program Files\R and from ~\Documents\R folders
    /// </summary>
    public class PackagesCompletionProvider : IRCompletionListProvider, IRHelpSearchTermProvider {
        private readonly object _glyph;
        private readonly IPackageIndex _packageIndex;

        public PackagesCompletionProvider(IPackageIndex packageIndex, IImageService imageService) {
            _packageIndex = packageIndex;
            _glyph = imageService.GetImage(ImageType.Library);
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<ICompletionEntry> GetEntries(IRIntellisenseContext context, string prefixFilter = null) {
            return _packageIndex.Packages
                .Select(p => new EditorCompletionEntry(p.Name, p.Name, p.Description, _glyph))
                .ToList();
        }
        #endregion

        #region IRHelpSearchTermProvider
        public IReadOnlyCollection<string> GetEntries() {
            return _packageIndex.Packages.Select(p => p.Name).ToList();
        }
        #endregion
    }
}
