// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Editor.Projection {
    /// <summary>
    /// Represent a mapping between primary (view) buffer and 
    /// the contained (projected) language 
    /// </summary>
    public sealed class ProjectionMapping : TextRange {
        /// <summary>
        /// Text range in primary (view) buffer
        /// </summary>
        public ITextRange SourceRange { get; }

        /// <summary>
        /// Text range inside contained language buffer.
        /// </summary>
        public ITextRange ProjectionRange { get; }

        /// <summary>
        /// Start of mapping in primary (view) buffer.
        /// </summary>
        public int SourceStart => SourceRange.Start;

        /// <summary>
        /// Start of mapping in secondary (contained language) buffer.
        /// </summary>
        public int ProjectionStart => ProjectionRange.Start;

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
    }
}
