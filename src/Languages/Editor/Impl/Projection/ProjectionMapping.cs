// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Languages.Core.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// Represent a mapping between primary (view) buffer and 
    /// the contained (projected) language 
    /// </summary>
    public sealed class ProjectionMapping : TextRange, IPropertyOwner {
        /// <summary>
        /// Text range in primary (view) buffer
        /// </summary>
        public ITextRange SourceRange { get; private set; }

        /// <summary>
        /// Text range inside contained language buffer.
        /// </summary>
        public ITextRange ProjectionRange { get; private set; }

        /// <summary>
        /// Start of mapping in primary (view) buffer.
        /// </summary>
        public int SourceStart => SourceRange.Start;

        /// <summary>
        /// Start of mapping in secondary (contained language) buffer.
        /// </summary>
        public int ProjectionStart => ProjectionRange.Start;

        private PropertyCollection _properties = new PropertyCollection();

        /// <summary>
        /// Creates projection mapping
        /// </summary>
        /// <param name="sourceStart">Mapping start in the primary (view) buffer</param>
        /// <param name="projectionStart">Mapping start in contained language buffer</param>
        /// <param name="length">Mapping length</param>
        /// <param name="trailingInclusion">Trailing content inclusion rule</param>
        public ProjectionMapping(int sourceStart, int projectionStart, int length) :
            base(sourceStart, length) {
            SourceRange = new TextRange(sourceStart, length);
            ProjectionRange = new TextRange(projectionStart, length);
        }

        public static int CompareViaSourceLocation(ITextRange first, ITextRange second) {
            return first.Start.CompareTo(second.Start);
        }

        public static int CompareViaProjectionLocation(ProjectionMapping first, ProjectionMapping second) {
            return first.ProjectionStart.CompareTo(second.ProjectionStart);
        }

        #region IPropertyOwner Members
        public PropertyCollection Properties => _properties;
        #endregion
    }
}
