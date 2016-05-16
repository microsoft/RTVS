// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.Languages.Editor.Projection {
    public interface IProjectionBufferManager: IDisposable {
        IProjectionBuffer ProjectionBuffer { get; }
        void SetTextAndMappings(string text, IReadOnlyList<ProjectionMapping> mappings);
    }
}
