using System.Windows.Forms;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    internal static class ProjectIconProvider {
        private static IVsImageMonikerImageList _monikerImageList;
        private static ImageList _imageList;

        public static ImageMoniker ProjectNodeImage { get; private set; }
        public static ImageMoniker RFileNodeImage { get; private set; }
        public static ImageMoniker RDataFileNodeImage { get; private set; }

        /// <summary>
        /// Creates image list and image monikers for project icons.
        /// Must be called on UI thread.
        /// </summary>
        public static void LoadProjectImages() {
            if (_monikerImageList == null) {
                IVsImageService2 imageService = VsAppShell.Current.GetGlobalService<IVsImageService2>(typeof(SVsImageService));

                if(_imageList == null) {
                    _imageList = new ImageList();
                }

                _imageList.Images.Add(Resources.RProjectNode);
                _imageList.Images.Add(Resources.RFileNode);
                _imageList.Images.Add(Resources.RDataNode);

                _monikerImageList = imageService.CreateMonikerImageListFromHIMAGELIST(_imageList.Handle);
                imageService.AddCustomImageList(_monikerImageList);

                ImageMoniker[] monikers = new ImageMoniker[3];
                _monikerImageList.GetImageMonikers(0, 3, monikers);

                ProjectNodeImage = monikers[0];
                RFileNodeImage = monikers[1];
                RDataFileNodeImage = monikers[2];
            }
        }

        public static void Close() {
            if(_imageList != null) {
                _imageList.Dispose();
                _imageList = null;
                _monikerImageList = null;
            }
        }
    }
}
