// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.Common.Core.Imaging;
using Microsoft.R.Support.Help;

namespace Microsoft.R.Editor.Completions.Providers {
    /// <summary>
    /// Provides list of installed packages for completion inside 
    /// library(...) statement. List of packages is  obtained from 
    /// ~\Program Files\R and from ~\Documents\R folders
    /// </summary>
    [Export(typeof(IRHelpSearchTermProvider))]
    public class PackagesCompletionProvider : IRCompletionListProvider, IRHelpSearchTermProvider {
        private readonly ImageSource _glyph;
        private readonly IPackageIndex _packageIndex;

        [ImportingConstructor]
        public PackagesCompletionProvider(IPackageIndex packageIndex, IImageService imageService) {
            _packageIndex = packageIndex;
            _glyph = imageService.GetImage(ImageType.Library) as ImageSource;
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            return _packageIndex.Packages
                .Select(p => new RCompletion(p.Name, p.Name, p.Description, _glyph))
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
