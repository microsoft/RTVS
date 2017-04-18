// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    public interface IProjectSystemServices {
        /// <summary>
        /// Locates current solution instance
        /// </summary>
        /// <returns></returns>
        EnvDTE.Solution GetSolution();

        /// <summary>
        /// Locates project that is currently selected in Solution Explorer
        /// </summary>
        T GetSelectedProject<T>() where T: class;

        /// <summary>
        /// Locates project that is currently active in Solution Explorer
        /// </summary>
        EnvDTE.Project GetActiveProject();

        /// <summary>
        /// Adds new item to the current project
        /// </summary>
        void AddNewItem(string templateName, string name, string extension, string destinationPath);

        /// <summary>
        /// Given folder, prefix and extension generates unique file name in the project folder.
        /// </summary>
        string GetUniqueFileName(string folder, string prefix, string extension, bool appendUnderscore = false);

        /// <summary>
        /// Retrieves folder name of the project item templates
        /// </summary>
        string GetProjectItemTemplatesFolder();

        /// <summary>
        /// Enumerates all files in the project traversing into sub folders
        /// and items that have child elements.
        /// </summary>
        IEnumerable<string> GetProjectFiles(EnvDTE.Project project);

        /// <summary>
        /// Locates project by name
        /// </summary>
        EnvDTE.Project GetProject(string projectName);
    }
}
