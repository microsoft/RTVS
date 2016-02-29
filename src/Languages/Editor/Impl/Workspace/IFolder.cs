// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;

namespace Microsoft.Languages.Editor.Workspace {
    public interface IFolder {
        /// <summary>
        /// Folder name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Parent folder (null of root)
        /// </summary>
        IFolder Parent { get; }

        /// <summary>
        /// Child folders
        /// </summary>
        ReadOnlyCollection<IFolder> Folders { get; }

        /// <summary>
        /// Folder files
        /// </summary>
        ReadOnlyCollection<IFile> Files { get; }
    }
}
