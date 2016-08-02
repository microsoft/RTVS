// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Configuration;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal static class ProjectExtensions {
        public static async Task<IEnumerable<IConfigurationSetting>> GetDatabaseConnections(this ConfiguredProject configuredProject, IProjectConfigurationSettingsProvider provider) {
            var dict = new Dictionary<string, string>();
            if (configuredProject != null) {
                using (var access = await provider.OpenProjectSettingsAccessAsync(configuredProject)) {
                    return access.Settings
                            .Where(s => s.EditorType.EqualsOrdinal(ConnectionStringEditor.ConnectionStringEditorName));
                }
            }
            return Enumerable.Empty<IConfigurationSetting>();
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
