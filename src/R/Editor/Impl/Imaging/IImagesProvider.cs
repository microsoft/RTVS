using System.Windows.Media;

namespace Microsoft.R.Editor.Imaging {
    /// <summary>
    /// Provides editor with images for the completion list.
    /// Exported via MEF for all content types.
    /// </summary>
    public interface IImagesProvider {
        /// <summary>
        /// Returns image source given name of the image moniker
        /// such as name from http://glyphlist.azurewebsites.net/knownmonikers
        /// </summary>
        ImageSource GetImage(string name);

        /// <summary>
        /// Given file name returns icon depending on the file extension
        /// </summary>
        ImageSource GetFileIcon(string file);
    }
}
