// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.DragDrop;
using Microsoft.R.Host.Client.Extensions;
using static System.FormattableString;

namespace Microsoft.R.Editor.DragDrop {
    public static class DataObject {
        public static string GetPlainText(this IDataObject dataObject, string projectFolder) {
            var flags = dataObject.GetFlags();
            if ((flags & DataObjectFlags.ProjectItems) != 0) {
                return dataObject.TextFromProjectItems(projectFolder);
            }
            return string.Empty;
        }

        private static string TextFromProjectItems(this IDataObject dataObject, string projectFolder) {
            var items = dataObject.GetProjectItems();
            var fileNames = items.Select(n => n.FileName);

            var sb = new StringBuilder();
            foreach(var item in items) {
                var relative = item.FileName.MakeRRelativePath(projectFolder);
                if (Path.GetExtension(item.FileName).EqualsIgnoreCase(".R")) {
                    // Convert to R project relative
                     sb.AppendLine(Invariant($"source('{relative}')"));
                } else {
                    sb.AppendLine(Invariant($"'{relative}'"));
                }
            }
            return sb.ToString();
        }
    }
}
