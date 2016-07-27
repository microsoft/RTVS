// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.R.Package.ProjectSystem;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    /// <summary>
    /// Represents persistent settings for the SQL stored procedure publishing dialog.
    /// </summary>
    internal class SqlSProcPublishSettings {
        /// <summary>
        /// List of files
        /// </summary>
        public IList<string> Files { get; set; } = new List<string>();

        /// <summary>
        /// List of stored procedure names
        /// </summary>
        public IDictionary<string, string> SProcNames => new Dictionary<string, string>();

        /// <summary>
        /// Target SQL table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Target database project name
        /// </summary>
        public string TargetProject { get; set; }

        /// <summary>
        /// Determines where to place R code in SQL
        /// </summary>
        public RCodePlacement CodePlacement { get; set; } = RCodePlacement.Inline;

        private IReadOnlyCollection<string> GetDatabaseProjectsInSolution(IProjectSystemServices pss) {
            var solution = pss.GetSolution();
            var projects = new List<string>();
            foreach (EnvDTE.Project project in solution.Projects) {
                foreach (var prop in project.Properties) {
                }
                projects.Add(project.Name);
            }
            return projects;
        }
    }
}