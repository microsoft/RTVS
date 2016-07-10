// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Editor.Workspace {
    /// <summary>
    /// Provides services related to the workspace (project).
    /// Exported via MEF by the host application.
    /// </summary>
    public interface IWorkspaceServices {
        string ActiveProjectPath { get; }
    }
}
