using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.ProjectSystem.Utilities.Designers;
using Microsoft.VisualStudio.R.Package;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project {

    [Export(typeof(IProjectTreeModifier))]
    [AppliesTo("RTools")]
    internal sealed class ProjectTreeModifier : IProjectTreeModifier {
        private static IVsImageMonikerImageList _monikerImageList;
        private static ImageList _imageList = new ImageList();
        private static ImageMoniker _projectNodeImage;
        private static ImageMoniker _rFileNodeImage;
        private static ImageMoniker _rDataFileNodeImage;

        public IProjectTree ApplyModifications(IProjectTree tree, IProjectTreeProvider projectTreeProvider) {
            InitProjectImages();

            if (tree != null) {
                if (tree.Capabilities.Contains(ProjectTreeCapabilities.ProjectRoot)) {
                    tree = tree.SetIcon(_projectNodeImage.ToProjectSystemType());
                }
                else if(tree.Capabilities.Contains(ProjectTreeCapabilities.FileOnDisk)) {
                    string ext = Path.GetExtension(tree.FilePath).ToLowerInvariant();
                    if(ext == ".r") {
                        tree = tree.SetIcon(_rFileNodeImage.ToProjectSystemType());
                    }
                    else if (ext == ".rdata" || ext == ".rhistory") {
                        tree = tree.SetIcon(_rDataFileNodeImage.ToProjectSystemType());
                    }
                }
            }
            return tree;
        }

        private void InitProjectImages() {

            if (_monikerImageList == null) {
                IVsImageService2 imageService = AppShell.Current.GetGlobalService<IVsImageService2>(typeof(SVsImageService));

                _imageList.Images.Add(Resources.RProjectNode);
                _imageList.Images.Add(Resources.RFileNode);
                _imageList.Images.Add(Resources.RDataNode);

                _monikerImageList = imageService.CreateMonikerImageListFromHIMAGELIST(_imageList.Handle);
                imageService.AddCustomImageList(_monikerImageList);

                ImageMoniker[] monikers = new ImageMoniker[3];
                _monikerImageList.GetImageMonikers(0, 3, monikers);

                _projectNodeImage = monikers[0];
                _rFileNodeImage = monikers[1];
                _rDataFileNodeImage = monikers[2];
            }
        }
    }
}
