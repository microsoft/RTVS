// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Media;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.Languages.Editor.Imaging {
    /// <summary>
    /// Equivalent of IGlyphService but allows concurrent access to glyphs
    /// from multiple threads which is normal in unit test environment.
    /// </summary>
    public static class GlyphService {
        private static readonly object _lock = new object();

        public static ImageSource GetGlyph(StandardGlyphGroup @group, StandardGlyphItem item, ICompositionCatalog compositionCatalog) {
            lock (_lock) {
                var glyphService = compositionCatalog.ExportProvider.GetExportedValue<IGlyphService>();
                return glyphService.GetGlyph(group, item);
            }
        }
    }
}
