// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Languages.Editor.Workspace;

namespace Microsoft.VisualStudio.R.Package.Workspace {
    public interface IVsWorkspaceItem : IWorkspaceItem {
        /// <summary>
        /// Returns Visual Studio hierarchy this item belongs to
        /// </summary>
        IVsHierarchy Hierarchy { get; }

        /// <summary>
        /// Visual Studio item id in the hierarchy
        /// </summary>
        VSConstants.VSITEMID ItemId { get; }
    }
}
