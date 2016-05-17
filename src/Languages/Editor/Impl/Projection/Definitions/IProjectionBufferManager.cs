// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.Languages.Editor.Projection {
    //  Graph:
    //      View Buffer [ContentType = RMD Projection]
    //        |      \
    //        |    Secondary [ContentType = R]
    //        |      /
    //       Disk Buffer [ContentType = RMD]

    public interface IProjectionBufferManager: IDisposable {
        IProjectionBuffer ViewBuffer { get; }
        IProjectionBuffer SecondaryProjectionBuffer { get; }
        void SetProjectionMappings(string secondaryContent, IReadOnlyList<ProjectionMapping> mappings);
    }
}
