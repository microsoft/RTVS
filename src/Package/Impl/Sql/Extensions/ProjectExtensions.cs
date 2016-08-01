// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal static class ProjectExtensions {
        public static async Task<IEnumerable<string>> GetDbConnections(this IVsHierarchy hierarchy, IProjectConfigurationSettingsProvider provider) {
            var configuredProject = hierarchy?.GetConfiguredProject();
            if (configuredProject != null) {
                using (var access = await provider.OpenProjectSettingsAccessAsync(configuredProject)) {
                    return access.Settings
                            .Where(s => s.EditorType.EqualsOrdinal(ConnectionStringEditor.ConnectionStringEditorName))
                            .Select(c => c.Value);
                }
            }
            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Retrieves list of files that represent stored procedures in R
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetSProcFiles(this EnvDTE.Project project, IProjectSystemServices pss) {
            var rFiles = pss.GetProjectFiles(project).Where(x => Path.GetExtension(x).EqualsIgnoreCase(".R"));
            var sqlFiles = new HashSet<string>(pss.GetProjectFiles(project).Where(x => Path.GetExtension(x).EqualsIgnoreCase(".sql")));
            return rFiles.Where(x =>
                        sqlFiles.Contains(x.ToQueryFilePath(), StringComparer.OrdinalIgnoreCase) &&
                        sqlFiles.Contains(x.ToSProcFilePath(), StringComparer.OrdinalIgnoreCase));
        }
    }
}
