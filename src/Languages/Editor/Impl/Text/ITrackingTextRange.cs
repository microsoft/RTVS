// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Represents text range that traccks positions across text changes
    /// </summary>
    public interface ITrackingTextRange: IPlatformSpecificObject {
        int GetStartPoint(IEditorBufferSnapshot snapshot);
        int GetEndPoint(IEditorBufferSnapshot snapshot);
    }
}
