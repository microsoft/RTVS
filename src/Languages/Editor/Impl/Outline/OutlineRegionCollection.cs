using System;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.Outline {
    /// <summary>
    /// A collection of outline regions than match specific text buffer version
    /// </summary>
    public class OutlineRegionCollection : TextRangeCollection<OutlineRegion>, ICloneable {
        public int TextBufferVersion { get; internal set; }

        public OutlineRegionCollection(int textBufferVersion) {
            TextBufferVersion = textBufferVersion;
        }

        #region ICloneable
        public virtual object Clone() {
            var clone = new OutlineRegionCollection(TextBufferVersion);

            foreach (var item in this)
                clone.Add(item.Clone() as OutlineRegion);

            return clone;
        }
        #endregion
    }
}
