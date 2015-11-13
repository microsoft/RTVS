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

        public ImageSource GetImage(string name) {
            ImageSource ims = GetImageFromResources(name);
            if (ims == null) {
                ImageMoniker? im = FindKnownMoniker(name);
                ims = im.HasValue ? GetIconForImageMoniker(im.Value) : null;
            }
            return ims;
        }

        private ImageMoniker? FindKnownMoniker(string name) {
            ImageMoniker cached;
            if (_monikerCache.TryGetValue(name, out cached)) {
                return cached;
            }

            ImageMoniker? moniker = null;
            Type t = typeof(KnownMonikers);
            PropertyInfo info = t.GetProperty(name, typeof(ImageMoniker));
            if(info != null) {
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
