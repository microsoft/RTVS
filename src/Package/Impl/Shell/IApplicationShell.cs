// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Settings;
using Microsoft.Languages.Editor.Shell;

namespace Microsoft.VisualStudio.R.Package.Shell {
    /// <summary>
    /// Application shell provides access to services such as 
    /// composition container, export provider, global VS IDE
    /// services and so on.
    /// </summary>
    public interface IApplicationShell : IEditorShell {
        /// <summary>
        /// Retrieves global service from the host application.
        /// This method is not thread safe and should not be called 
        /// from async methods.
        /// </summary>
        /// <typeparam name="T">Service interface type such as IVsUIShell</typeparam>
        /// <param name="type">Service type if different from T, such as typeof(SVSUiShell)</param>
        /// <returns>Service instance of null if not found.</returns>
        T GetGlobalService<T>(Type type = null) where T : class;

        string BrowseForFileOpen(IntPtr owner, string filter, string initialPath = null, string title = null);

        string BrowseForFileSave(IntPtr owner, string filter, string initialPath = null, string title = null);
    }
}
