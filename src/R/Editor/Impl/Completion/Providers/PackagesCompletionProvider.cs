// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Support.Help;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
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
        public PackagesCompletionProvider(IPackageIndex packageIndex, IGlyphService glyphService) {
            _packageIndex = packageIndex;
            _glyph = glyphService.GetGlyphThreadSafe(StandardGlyphGroup.GlyphLibrary, StandardGlyphItem.GlyphItemPublic);
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
