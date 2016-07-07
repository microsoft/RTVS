// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Completion.Definitions;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Packages;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.R.Editor.Completion.Providers {
    /// <summary>
    /// Provides list of installed packages for completion inside 
    /// library(...) statement. List of packages is  obtained from 
    /// ~\Program Files\R and from ~\Documents\R folders
    /// </summary>
    [Export(typeof(IRHelpSearchTermProvider))]
    public class PackagesCompletionProvider : IRCompletionListProvider, IRHelpSearchTermProvider {
        private readonly ICoreShell _shell;
        private readonly IPackageIndex _packageIndex;

        [ImportingConstructor]
        public PackagesCompletionProvider(ICoreShell shell) {
            _shell = shell;
            _packageIndex = shell.ExportProvider.GetExportedValue<IPackageIndex>();
        }

        #region IRCompletionListProvider
        public bool AllowSorting { get; } = true;

        public IReadOnlyCollection<RCompletion> GetEntries(RCompletionContext context) {
            ImageSource glyph = GlyphService.GetGlyph(StandardGlyphGroup.GlyphLibrary, StandardGlyphItem.GlyphItemPublic, _shell);

            return _packageIndex.Packages
                .Select(p => new RCompletion(p.Name, p.Name, p.Description, glyph))
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
