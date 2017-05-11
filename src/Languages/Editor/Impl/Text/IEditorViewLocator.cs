// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Languages.Editor.Text {
    /// <summary>
    /// Application-provided service for location of views based on given editor buffer.
    /// </summary>
    public interface IEditorViewLocator {
        IEditorView GetPrimaryView(IEditorBuffer editorBuffer);
        IEnumerable<IEditorView> GetAllViews(IEditorBuffer editorBuffer);
    }
}
