// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Common.Core;

namespace Microsoft.Languages.Editor.DragDrop {
    public static class DataObjectExtensions {
        public static DataObjectFlags GetFlags(this IDataObject dataObject) {
            DataObjectFlags flags = DataObjectFlags.None;

            var formats = dataObject.GetFormats();
            foreach (var format in formats) {
                if (format.EqualsIgnoreCase(DataFormats.Text)) {
                    flags |= DataObjectFlags.Text;
                } else if (format.EqualsIgnoreCase(DataFormats.UnicodeText)) {
                    flags |= DataObjectFlags.Unicode;
                } else if (format.EqualsIgnoreCase(DataFormats.Html)) {
                    flags |= DataObjectFlags.Html;
                } else if (format.EqualsIgnoreCase(DataFormats.FileDrop)) {
                    flags |= DataObjectFlags.FileDrop;
                } else if (format.EqualsIgnoreCase(DataObjectFormats.MultiUrl)) {
                    flags |= DataObjectFlags.MultiUrl;
                } else if (format.EqualsIgnoreCase(DataObjectFormats.VSProjectItems)) {
                    flags |= DataObjectFlags.ProjectItems;
                }
            }
            return flags;
        }

        public static string TextFromUnicode(this IDataObject dataObject) {
            return dataObject.GetData(DataFormats.UnicodeText) as string;
        }

        public static string TextFromAnsi(this IDataObject dataObject) {
            return dataObject.GetData(DataFormats.Text) as string;
        }

        public static IEnumerable<ProjectItem> GetProjectItems(this IDataObject data) {
            if (data.IsProjectItems()) {
                using (Stream stream = data.GetData(DataObjectFormats.VSProjectItems) as MemoryStream) {
                    return ProjectDataObjectReader.GetData(stream);
                }
            }
            return Enumerable.Empty<ProjectItem>();
        }

        public static bool IsProjectItems(this IDataObject data) {
            return data != null && data.GetDataPresent(DataObjectFormats.VSProjectItems);
        }
    }
}
