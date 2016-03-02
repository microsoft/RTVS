// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.Languages.Editor.Workspace {
    public interface IFile {
        /// <summary>
        /// File name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Containing folder
        /// </summary>
        IFolder Folder { get; }
    }
}
