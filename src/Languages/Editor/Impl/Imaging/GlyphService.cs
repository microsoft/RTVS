using System.Windows.Media;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.Languages.Editor.Imaging
{
    /// <summary>
    /// Equivalent of IGlyphService but allows concurrent access to glyphs
    /// from multiple threads which is normal in unit test environment.
    /// </summary>
    public static class GlyphService
    {
        private static object _lock = new object();

        public static ImageSource GetGlyph(StandardGlyphGroup group, StandardGlyphItem item)
        {
            lock (_lock)
            {
                var glyphService = EditorShell.Current.ExportProvider.GetExport<IGlyphService>().Value;
                return glyphService.GetGlyph(group, item);
            }
        }
    }
}
