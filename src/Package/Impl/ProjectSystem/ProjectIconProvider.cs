// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Drawing;
using System.Windows.Forms;
using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    internal static class ProjectIconProvider {
        private static readonly Bitmap[] _bitmaps = new Bitmap[] {
            Resources.RProjectNode,
            Resources.RFileNode,
            Resources.RDataFileNode,
            Resources.RdFileNode,
            Resources.RMdFileNode,
            Resources.SQLFileNode,
            Resources.ProcedureFileNode
        };

        private static IVsImageMonikerImageList _monikerImageList;
        private static ImageList _imageList;

        public static ImageMoniker ProjectNodeImage { get; private set; }
        public static ImageMoniker RFileNodeImage { get; private set; }
        public static ImageMoniker RDataFileNodeImage { get; private set; }
        public static ImageMoniker RdFileNodeImage { get; private set; }
        public static ImageMoniker RMarkdownFileNodeImage { get; private set; }
        public static ImageMoniker SqlFileNodeImage { get; private set; }
        public static ImageMoniker SqlProcFileNodeImage { get; private set; }

        /// <summary>
        /// Creates image list and image monikers for project icons.
        /// Must be called on UI thread.
        /// </summary>
        public static void LoadProjectImages(IServiceContainer services) {
            services.MainThread().Assert();

            if (_monikerImageList == null) {
                IVsImageService2 imageService = services.GetService<IVsImageService2>(typeof(SVsImageService));

                _imageList = new ImageList();
                foreach (var b in _bitmaps) {
                    _imageList.Images.Add(b);
                }

                _monikerImageList = imageService.CreateMonikerImageListFromHIMAGELIST(_imageList.Handle);
                imageService.AddCustomImageList(_monikerImageList);

                ImageMoniker[] monikers = new ImageMoniker[_bitmaps.Length];
                _monikerImageList.GetImageMonikers(0, _bitmaps.Length, monikers);

                ProjectNodeImage = monikers[0];
                RFileNodeImage = monikers[1];
                RDataFileNodeImage = monikers[2];
                RdFileNodeImage = monikers[3];
                RMarkdownFileNodeImage = monikers[4];
                SqlFileNodeImage = monikers[5];
                SqlProcFileNodeImage = monikers[6];
            }
        }

        public static void Close() {
            if (_imageList != null) {
                _imageList.Dispose();
                _imageList = null;
                _monikerImageList = null;
            }
        }
    }
}
