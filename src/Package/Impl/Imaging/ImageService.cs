// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Wpf.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Imaging {
    internal sealed class ImageService : IImageService {
        private readonly IServiceContainer _services;
        private readonly IGlyphService _glyphService;
        private readonly Dictionary<string, ImageMoniker> _monikerCache = new Dictionary<string, ImageMoniker>();
        private readonly Lazy<Dictionary<string, string>> _fileExtensionCache = Lazy.Create(CreateExtensionCache);

        public ImageService(IServiceContainer services) {
            _services = services;
            _glyphService = services.GetService<IGlyphService>();
        }

        public object GetImage(ImageType imageType) {
            switch (imageType) {
                case ImageType.Keyword:
                    return _glyphService.GetGlyph(StandardGlyphGroup.GlyphKeyword, StandardGlyphItem.GlyphItemPublic);
                case ImageType.Function:
                    return _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
                case ImageType.Variable:
                    return _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupVariable, StandardGlyphItem.GlyphItemPublic);
                case ImageType.Method:
                    return _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupMethod, StandardGlyphItem.GlyphItemPublic);
                case ImageType.Constant:
                    return _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupConstant, StandardGlyphItem.GlyphItemPublic);
                case ImageType.Library:
                    return _glyphService.GetGlyph(StandardGlyphGroup.GlyphLibrary, StandardGlyphItem.GlyphItemPublic);
                case ImageType.ValueType:
                    return _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupValueType, StandardGlyphItem.GlyphItemPublic);
                case ImageType.Snippet:
                    return _glyphService.GetGlyph(StandardGlyphGroup.GlyphCSharpExpansion, StandardGlyphItem.GlyphItemPublic);
                case ImageType.OpenFolder:
                    return _glyphService.GetGlyph(StandardGlyphGroup.GlyphOpenFolder, StandardGlyphItem.GlyphItemPublic);
                case ImageType.ClosedFolder:
                    return _glyphService.GetGlyph(StandardGlyphGroup.GlyphClosedFolder, StandardGlyphItem.GlyphItemPublic);
                case ImageType.Intrinsic:
                    return _glyphService.GetGlyph(StandardGlyphGroup.GlyphGroupIntrinsic, StandardGlyphItem.GlyphItemPublic);
                case ImageType.File:
                case ImageType.Document:
                    return GetImage("Document");
            }
            return null;
        }

        /// <summary>
        /// Returns image source given name of the image moniker
        /// such as name from http://glyphlist.azurewebsites.net/knownmonikers
        /// </summary>
        public object GetImage(string name) {
            var ims = GetImageFromResources(name);
            if (ims == null) {
                var im = FindKnownMoniker(name);
                ims = im.HasValue ? GetIconForImageMoniker(im.Value) : null;
            }
            return ims;
        }

        /// <summary>
        /// Given file name returns icon depending on the file extension
        /// </summary>
        public object GetFileIcon(string file) {
            var ext = Path.GetExtension(file);
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
            var t = typeof(KnownMonikers);
            var info = t.GetProperty(name, typeof(ImageMoniker));
            if (info != null) {
                var mi = info.GetGetMethod(nonPublic: false);
                moniker = (ImageMoniker)mi.Invoke(null, new object[0]);
                _monikerCache[name] = moniker.Value;
            }

            return moniker;
        }

        public ImageSource GetIconForImageMoniker(ImageMoniker imageMoniker) {
            var imageService = _services.GetService<IVsImageService2>(typeof(SVsImageService));
            ImageSource glyph = null;

            var imageAttributes = new ImageAttributes();
            imageAttributes.Flags = (uint)_ImageAttributesFlags.IAF_RequiredFlags;
            imageAttributes.ImageType = (uint)_UIImageType.IT_Bitmap;
            imageAttributes.Format = (uint)_UIDataFormat.DF_WPF;
            imageAttributes.LogicalHeight = 16;// IconHeight,
            imageAttributes.LogicalWidth = 16;// IconWidth,
            imageAttributes.StructSize = Marshal.SizeOf(typeof(ImageAttributes));

            var result = imageService.GetImage(imageMoniker, imageAttributes);

            Object data = null;
            if (result.get_Data(out data) == VSConstants.S_OK) {
                glyph = data as ImageSource;
                if (glyph != null) {
                    glyph.Freeze();
                }
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
                using (var memory = new MemoryStream()) {
                    bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                    memory.Position = 0;
                    source = BitmapImageFactory.Load(memory);
                }
            }

            return source;
        }

        private static Dictionary<string, string> CreateExtensionCache() {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
