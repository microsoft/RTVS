// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Common.Wpf.Imaging;
using Microsoft.R.Editor.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Imaging {

    [Export(typeof(IImagesProvider))]
    internal sealed class ImagesProvider : IImagesProvider {
        private readonly ConcurrentDictionary<string, ImageMoniker> _monikerCache = new ConcurrentDictionary<string, ImageMoniker>();
        private readonly Lazy<ConcurrentDictionary<string, string>> _fileExtensionCache = Lazy.Create(CreateExtensionCache);

        private static IVsImageService2 _imageService;

        public ImagesProvider() {
            _imageService = _imageService ?? VsAppShell.Current.GetGlobalService<IVsImageService2>(typeof(SVsImageService));
        }

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
            if (_fileExtensionCache.Value.ContainsKey(ext)) {
                return GetImage(_fileExtensionCache.Value[ext]);
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
            ImageSource glyph = null;

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.Flags = (uint)_ImageAttributesFlags.IAF_RequiredFlags;
            imageAttributes.ImageType = (uint)_UIImageType.IT_Bitmap;
            imageAttributes.Format = (uint)_UIDataFormat.DF_WPF;
            imageAttributes.LogicalHeight = 16;// IconHeight,
            imageAttributes.LogicalWidth = 16;// IconWidth,
            imageAttributes.StructSize = Marshal.SizeOf(typeof(ImageAttributes));

            var result = _imageService.GetImage(imageMoniker, imageAttributes);

            object data;
            if (result.get_Data(out data) == VSConstants.S_OK) {
                glyph = data as ImageSource;
                glyph?.Freeze();
            }

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
                case "RDataFileNode":
                    bmp = Resources.RDataFileNode;
                    break;
                case "RdFileNode":
                    bmp = Resources.RdFileNode;
                    break;
                case "RMdFileNode":
                    bmp = Resources.RMdFileNode;
                    break;
                case "SQLFileNode":
                    bmp = Resources.SQLFileNode;
                    break;
                case "ProcedureFileNode":
                    bmp = Resources.ProcedureFileNode;
                    break;
            }

            if (bmp != null) {
                using (MemoryStream memory = new MemoryStream()) {
                    bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                    memory.Position = 0;
                    source = BitmapImageFactory.Load(memory);
                }
            }

            return source;
        }

        private static ConcurrentDictionary<string, string> CreateExtensionCache() {
            var dict = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            dict[".r"] = "RFileNode";
            dict[".rproj"] = "RProjectNode";
            dict[".rdata"] = "RDataFileNode";
            dict[".md"] = "MarkdownFile";
            dict[".rmd"] = "RMdFileNode";
            dict[".html"] = "HTMLFile";
            dict[".css"] = "StyleSheet";
            dict[".xml"] = "XMLFile";
            dict[".txt"] = "TextFile";
            dict[".docx"] = "OfficeWord2013";
            dict[".xlsx"] = "OfficeExcel2013";
            dict[".py"] = "PYFileNode";
            dict[".cpp"] = "CPPFileNode";
            dict[".cxx"] = "CPPFileNode";
            dict[".rcpp"] = "CPPFileNode";
            dict[".c"] = "CFile";
            dict[".h"] = "CPPHeaderFile";
            dict[".hpp"] = "CPPHeaderFile";
            dict[".sql"] = "SQLFileNode";
            dict[".rd"] = "RdFileNode";

            return dict;
        }
    }
}
