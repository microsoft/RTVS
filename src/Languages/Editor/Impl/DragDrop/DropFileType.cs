// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Microsoft.Languages.Editor.DragDrop {
    internal enum DropFileType {
        Image,
        Css,
        CssExtension,
        JavaScript,
        Video,
        Audio,
        TypeScript,
        Font,
        Other,
        Ignore
    }

    internal static class DropFileTypeHelper {
        private static Dictionary<string, string> _mimeTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, DropFileType> _fileTypeMap = new Dictionary<string, DropFileType>(StringComparer.OrdinalIgnoreCase);

        static DropFileTypeHelper() {
            _fileTypeMap[".gif"] = DropFileType.Image;
            _fileTypeMap[".jpg"] = DropFileType.Image;
            _fileTypeMap[".jpeg"] = DropFileType.Image;
            _fileTypeMap[".bmp"] = DropFileType.Image;
            _fileTypeMap[".wmf"] = DropFileType.Image;
            _fileTypeMap[".xbm"] = DropFileType.Image;
            _fileTypeMap[".art"] = DropFileType.Image;
            _fileTypeMap[".png"] = DropFileType.Image;
            _fileTypeMap[".ico"] = DropFileType.Image;
            _fileTypeMap[".svg"] = DropFileType.Image;
            _fileTypeMap[".webp"] = DropFileType.Image;

            _fileTypeMap[".css"] = DropFileType.Css;
            _fileTypeMap[".less"] = DropFileType.CssExtension;
            _fileTypeMap[".scss"] = DropFileType.CssExtension;
            _fileTypeMap[".js"] = DropFileType.JavaScript;

            _fileTypeMap[".ogg"] = DropFileType.Audio;
            _fileTypeMap[".mp3"] = DropFileType.Audio;
            _fileTypeMap[".aac"] = DropFileType.Audio;
            _fileTypeMap[".wav"] = DropFileType.Audio;
            _fileTypeMap[".flac"] = DropFileType.Audio;
            _fileTypeMap[".m4p"] = DropFileType.Audio;

            _fileTypeMap[".mp4"] = DropFileType.Video;
            _fileTypeMap[".webm"] = DropFileType.Video;
            _fileTypeMap[".mv4"] = DropFileType.Video;
            _fileTypeMap[".m4v"] = DropFileType.Video;
            _fileTypeMap[".mov"] = DropFileType.Video;
            _fileTypeMap[".qtm"] = DropFileType.Video;
            _fileTypeMap[".wmv"] = DropFileType.Video;
            _fileTypeMap[".ogv"] = DropFileType.Video;

            _fileTypeMap[".ts"] = DropFileType.TypeScript;

            _fileTypeMap[".eot"] = DropFileType.Font;
            _fileTypeMap[".otf"] = DropFileType.Font;
            _fileTypeMap[".ttf"] = DropFileType.Font;
            _fileTypeMap[".woff"] = DropFileType.Font;
            // SVG could be a font, but it's already an image

            _mimeTypeMap[".mp4"] = "mp4";
            _mimeTypeMap[".m4p"] = "mp4";
            _mimeTypeMap[".mp3"] = "mpeg";
            _mimeTypeMap[".webm"] = "webm";
            _mimeTypeMap[".ogg"] = "ogg";
            _mimeTypeMap[".ogv"] = "ogg";
            _mimeTypeMap[".mov"] = "quicktime";
            _mimeTypeMap[".mv4"] = "quicktime";
            _mimeTypeMap[".m4v"] = "quicktime";
            _mimeTypeMap[".qtm"] = "quicktime";
            _mimeTypeMap[".wmv"] = "x-ms-wmv";
            _mimeTypeMap[".flv"] = "x-flv";
            _mimeTypeMap[".flac"] = "x-flac";
            _mimeTypeMap[".aac"] = "aac";
        }

        /// <summary>
        /// Returns soure type for a given extensions as in &lt;source src="video/wbm">
        /// </summary>
        public static string GetMimeType(string file) {
            string ext = Path.GetExtension(file).ToLower(CultureInfo.CurrentCulture);
            if (_mimeTypeMap.ContainsKey(ext)) {
                return _mimeTypeMap[ext];
            }
            return null;
        }

        public static DropFileType GetDropFileType(string file) {
            DropFileType dropFileType;
            string ext = Path.GetExtension(file).ToLower(CultureInfo.CurrentCulture);
            if (string.IsNullOrEmpty(ext)) {
                dropFileType = DropFileType.Ignore;
            } else if (_fileTypeMap.TryGetValue(ext, out dropFileType)) {
                if (dropFileType == DropFileType.TypeScript) {
                    if (file.EndsWith(".d.ts", StringComparison.OrdinalIgnoreCase)) {
                        dropFileType = DropFileType.Ignore;
                    }
                }
            } else {
                dropFileType = DropFileType.Other;
            }
            return dropFileType;
        }
    }
}
