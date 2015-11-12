using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.R.Editor.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Imaging {

    [Export(typeof(IImagesProvider))]
    internal sealed class ImagesProvider : IImagesProvider {
        private Dictionary<string, ImageMoniker> _monikerCache = new Dictionary<string, ImageMoniker>();

        /// <summary>
        /// Returns image source given name of the image moniker
        /// such as name from http://glyphlist.azurewebsites.net/knownmonikers
        /// </summary>
        public ImageSource GetImage(string name) {
            ImageSource ims = GetImageFromResources(name);
            if (ims == null) {
                ImageMoniker? im = FindKnownMoniker(name);
                ims = im.HasValue ? GetIconForImageMoniker(im.Value) : null;
            }
            return ims;
        }

        /// <summary>
        /// Given file name returns icon depending on the file extension
        /// </summary>
        public ImageSource GetFileIcon(string file) {
            string ext = Path.GetExtension(file);
            if (ext == ".R" || ext == ".r") {
                return GetImage("RFileNode");
            }
            if (ext.Equals(".rproj", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("RProjectNode");
            }
            if (ext.Equals(".rdata", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("RDataNode");
            }
            if (ext.Equals(".md", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("MarkdownFile");
            }
            if (ext.Equals(".rmd", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("MarkdownFile");
            }
            if (ext.Equals(".html", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("HTMLFile");
            }
            if (ext.Equals(".css", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("StyleSheet");
            }
            if (ext.Equals(".xml", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("XMLFile");
            }
            if (ext.Equals(".txt", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("TextFile");
            }
            if (ext.Equals(".docx", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("OfficeWord2013");
            }
            if (ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("OfficeExcel2013");
            }
            if (ext.Equals(".py", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("PYFileNode");
            }
            if (ext.Equals(".cpp", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".cxx", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".rcpp", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("CPPFileNode");
            }
            if (ext.Equals(".c", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("CFile");
            }
            if (ext.Equals(".h", StringComparison.OrdinalIgnoreCase) || ext.Equals(".hpp", StringComparison.OrdinalIgnoreCase)) {
                return GetImage("CPPHeaderFile");
            }
            return GetImage("Document");
        }

        private ImageMoniker? FindKnownMoniker(string name) {
            ImageMoniker cached;
            if (_monikerCache.TryGetValue(name, out cached)) {
                return cached;
            }

            ImageMoniker? moniker = null;
            Type t = typeof(KnownMonikers);
            PropertyInfo info = t.GetProperty(name, typeof(ImageMoniker));
            if (info != null) {
                MethodInfo mi = info.GetGetMethod(nonPublic: false);
                moniker = (ImageMoniker)mi.Invoke(null, new object[0]);
                _monikerCache[name] = moniker.Value;
            }

            return moniker;
        }

        public static ImageSource GetIconForImageMoniker(ImageMoniker imageMoniker) {
            IVsImageService2 imageService = AppShell.Current.GetGlobalService<IVsImageService2>(typeof(SVsImageService));

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.Flags = (uint)_ImageAttributesFlags.IAF_RequiredFlags;
            imageAttributes.ImageType = (uint)_UIImageType.IT_Bitmap;
            imageAttributes.Format = (uint)_UIDataFormat.DF_WPF;
            imageAttributes.LogicalHeight = 16;// IconHeight,
            imageAttributes.LogicalWidth = 16;// IconWidth,
            imageAttributes.StructSize = Marshal.SizeOf(typeof(ImageAttributes));

            IVsUIObject result = imageService.GetImage(imageMoniker, imageAttributes);

            Object data;
            result.get_Data(out data);
            ImageSource glyph = data as ImageSource;
            glyph.Freeze();

            return glyph;
        }

        private ImageSource GetImageFromResources(string name) {
            Bitmap bmp = null;
            ImageSource source = null;

            switch (name) {
                case "RProjectNode":
                    bmp = Resources.RProjectNode;
                    break;
                case "RFileNode":
                    bmp = Resources.RFileNode;
                    break;
                case "RDataNode":
                    bmp = Resources.RDataNode;
                    break;
            }

            if (bmp != null) {
                using (MemoryStream memory = new MemoryStream()) {
                    bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    source = bitmapImage;
                }
            }

            return source;
        }
    }
}
