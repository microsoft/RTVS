// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Media;
using Microsoft.Common.Core.Shell;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.Languages.Editor.Imaging {
    /// <summary>
    /// Equivalent of IGlyphService but allows concurrent access to glyphs
    /// from multiple threads which is normal in unit test environment.
    /// </summary>
    public static class GlyphServiceExtensions {
        private static readonly object _lock = new object();

        public static ImageSource GetGlyphThreadSafe(this IGlyphService glyphService, StandardGlyphGroup @group, StandardGlyphItem item) {
            lock (_lock) {
                return glyphService.GetGlyph(group, item);
            }
        }
    }
}
