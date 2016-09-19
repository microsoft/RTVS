// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Languages.Editor.DragDrop;
using Microsoft.R.Host.Client.Extensions;
using static System.FormattableString;

namespace Microsoft.R.Editor.DragDrop {
    public static class DataObject {
        public static string GetPlainText(this IDataObject dataObject, string projectFolder, DragDropKeyStates keystates) {
            var flags = dataObject.GetFlags();
            if ((flags & DataObjectFlags.ProjectItems) != 0) {
                return dataObject.TextFromProjectItems(projectFolder, keystates);
            }
            return string.Empty;
        }

        private static string TextFromProjectItems(this IDataObject dataObject, string projectFolder, DragDropKeyStates keystates) {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var item in dataObject.GetProjectItems()) {

                var relative = item.FileName.MakeRRelativePath(projectFolder);
                var ext = Path.GetExtension(item.FileName).ToLowerInvariant();
                switch (ext) {
                    case ".r":
                        if ((keystates & DragDropKeyStates.ControlKey) != 0) {
                            sb.Append(GetFileContent(item.FileName));
                        } else {
                            var str = first ? Environment.NewLine : string.Empty;
                            sb.AppendLine(Invariant($"{str}source('{relative}')"));
                            first = false;
                        }
                        break;
                    case ".sql":
                        if ((keystates & DragDropKeyStates.ControlKey) != 0) {
                            sb.Append(Invariant($"'{GetFileContent(item.FileName)}'"));
                        } else {
                            sb.Append(Invariant($@"iconv(paste(readLines('{relative}', encoding = 'UTF-8', warn = FALSE), collapse='\n'), from = 'UTF-8', to = 'ASCII', sub = '')"));
                        }
                        break;
                    default:
                        sb.Append(Invariant($"'{relative}'"));
                        break;
                }
            }
            return sb.ToString();
        }

        private static string GetFileContent(string file) {
            try {
                return File.ReadAllText(file).Trim();
            } catch (IOException) { } catch (UnauthorizedAccessException) { }
            return string.Empty;
        }
    }
}
