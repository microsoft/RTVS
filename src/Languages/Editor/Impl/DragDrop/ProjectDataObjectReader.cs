// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Microsoft.Languages.Editor.DragDrop {
    public class ProjectItem {
        public Guid ProjectGuid { get; }
        public string ProjectFileName { get; }
        public string FileName { get; }

        public ProjectItem(Guid projectGuid, string projectFileName, string itemFileName) {
            ProjectGuid = projectGuid;
            ProjectFileName = projectFileName;
            FileName = itemFileName;
        }
    }

    internal static class ProjectDataObjectReader {
        /// <summary>
        /// Reads and parses memory stream representing CF_VSSTGPROJECTITEMS format
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        internal static IReadOnlyList<ProjectItem> GetData(Stream stream) {
            // DROPFILES structure (20 bytes)
            // String\0
            // String\0
            // ...
            // String\0\0
            //
            // One string per each drag-dropped Solution Explorer node. 
            // The fWide member in the DROPFILES structure tells us if the string is encoded in Unicode or ASCII.
            // Each string contains the following:
            // {project Guid} +”|”+ project file name +”|”+ drag-dropped file name

            var items = new List<ProjectItem>();
            if (stream != null && stream.Length > 0 && stream.CanRead) {
                BinaryReader reader = null;
                try {
                    var encoding = (BitConverter.IsLittleEndian ? Encoding.Unicode : Encoding.BigEndianUnicode);
                    reader = new BinaryReader(stream, encoding);

                    // Read the initial DROPFILES struct (20 bytes)
                    int filesOffset = reader.ReadInt32();
                    int pointX = reader.ReadInt32();
                    int pointY = reader.ReadInt32();
                    int fNC = reader.ReadInt32();

                    int wide = reader.ReadInt32();
                    if(wide == 0) { // We don't deal with ASCII
                        return items;
                    }

                    if (filesOffset > 0 && filesOffset < reader.BaseStream.Length) {
                        stream.Seek(filesOffset, SeekOrigin.Begin);

                        char lastChar = '\0';
                        var eachNodeStrings = new List<string>();
                        var fileNameString = new StringBuilder();
                        while (reader.BaseStream.Position < reader.BaseStream.Length) {
                            char c = reader.ReadChar();
                            if (c == '\0' && lastChar == '\0') {
                                break;
                            }
                            if (c == '\0') {
                                eachNodeStrings.Add(fileNameString.ToString());
                                fileNameString.Clear();
                            } else {
                                fileNameString.Append(c);
                            }
                            lastChar = c;
                        }

                        foreach (string eachNode in eachNodeStrings) {
                            string[] splitString = eachNode.Split('|');
                            Debug.Assert(splitString.Length == 3, "Expecting three parts in the solution explorer node data string.");
                            if (splitString.Length == 3) {
                                var item = new ProjectItem(new Guid(splitString[0]), splitString[1], splitString[2]);
                                items.Add(item);
                            }
                        }
                    }
                } finally {
                    if (reader != null) {
                        reader.Close();
                    }
                }
            }
            return items;
        }
    }
}