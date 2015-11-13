using System.Windows.Media;

namespace Microsoft.R.Editor.Imaging {
    /// <summary>
    /// Provides editor with images for the completion list.
    /// Exported via MEF for all content types.
    /// </summary>
    public interface IImagesProvider {
        ImageSource GetImage(string name);
    }
}
