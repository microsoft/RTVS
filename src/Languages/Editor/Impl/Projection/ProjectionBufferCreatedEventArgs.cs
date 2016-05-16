// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information

using System;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Projection {
    public sealed class ProjectionBufferCreatedEventArgs : EventArgs {
        public IContentType ContentType { get; }
        public ContainedLanguageProjectionBuffer ProjectionBuffer { get; }

        public ProjectionBufferCreatedEventArgs(IContentType contentType, ContainedLanguageProjectionBuffer projectionBuffer) {
            ProjectionBuffer = projectionBuffer;
            ContentType = contentType;
        }
    }
}
