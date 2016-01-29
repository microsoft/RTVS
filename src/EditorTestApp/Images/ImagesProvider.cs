using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.Languages.Editor.Imaging;
using Microsoft.R.Editor.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.Languages.Editor.Application.Images {
    [Export(typeof(IImagesProvider))]
    internal sealed class ImagesProvider : IImagesProvider {
        public ImageSource GetFileIcon(string file) {
            return GlyphService.GetGlyph(StandardGlyphGroup.GlyphAssembly, StandardGlyphItem.GlyphItemPublic);
        }

        public ImageSource GetImage(string name) {
            return GlyphService.GetGlyph(StandardGlyphGroup.GlyphAssembly, StandardGlyphItem.GlyphItemPublic);
        }
    }
}
