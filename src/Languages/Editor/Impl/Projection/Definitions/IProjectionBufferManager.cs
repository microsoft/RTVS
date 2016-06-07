// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.Languages.Editor.Projection {
    //  Graph:
    //      View Buffer [ContentType = RMD Projection]
    //        |      \
    //        |    Secondary [ContentType = R]
    //        |      /
    //       Disk Buffer [ContentType = RMD]

    public interface IProjectionBufferManager: IDisposable {
        /// <summary>
        /// Projection buffer that is presented in the view.
        /// Content type typically derives from 'projection'.
        /// </summary>
        IProjectionBuffer ViewBuffer { get; }

        /// <summary>
        /// Contained language buffer. Normally a projection buffer
        /// with content type of the contained language and spans
        /// mapped to the secondary language areas of the disk buffer
        /// or to inert text.
        /// </summary>
        IProjectionBuffer ContainedLanguageBuffer { get; }

        /// <summary>
        /// Buffer that contains original document that was loaded from disk.
        /// </summary>
        ITextBuffer DiskBuffer { get; }

        /// <summary>
        /// Sets projections for the secondary language
        /// </summary>
        /// <param name="secondaryContent">Contained language buffer content</param>
        /// <param name="mappings">Mappings that describe projections of contained language buffer to the view</param>
        void SetProjectionMappings(string secondaryContent, IReadOnlyList<ProjectionMapping> mappings);

        event EventHandler MappingsChanged;
    }
}
