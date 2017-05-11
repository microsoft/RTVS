// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.Outline {
    /// <summary>
    /// A collection of outline regions than match specific text buffer version
    /// </summary>
    public class OutlineRegionCollection : TextRangeCollection<OutlineRegion> {
        public int TextBufferVersion { get; internal set; }

        public OutlineRegionCollection(int textBufferVersion) {
            TextBufferVersion = textBufferVersion;
        }

        public OutlineRegionCollection Clone() {
            var clone = new OutlineRegionCollection(TextBufferVersion);

            foreach (var item in this) {
                clone.Add(item.Clone() as OutlineRegion);
            }

            return clone;
        }
    }
}
