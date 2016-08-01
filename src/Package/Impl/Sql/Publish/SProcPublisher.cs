// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Components.Sql;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    /// <summary>
    /// Provides functionality for publishing stored procedures to projects or databases
    /// </summary>
    internal sealed class SProcPublisher {
        private const string DacPacExtension = "dacpac";

        private readonly IApplicationShell _appShell;
        private readonly IProjectSystemServices _pss;
        private readonly IFileSystem _fs;
        private readonly IDacPackageServices _dacServices;

        public SProcPublisher(IApplicationShell appShell, IProjectSystemServices pss, IFileSystem fs, IDacPackageServices dacServices) {
            _appShell = appShell;
            _pss = pss;
            _fs = fs;
            _dacServices = dacServices;
        }

        public void Publish(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles) {
            switch (settings.TargetType) {
                case PublishTargetType.Project:
                    PublishToProject(settings, sprocFiles);
                    break;

                case PublishTargetType.Database:
                    PublishToDatabase(settings, sprocFiles);
                    break;

                default:
                    PublishToDacPac(settings, sprocFiles);
                    break;
            }
        }

        /// <summary>
        /// Packages stored procedures into a DACPAC.
        /// </summary>
        private void PublishToDacPac(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles) {
            var project = _pss.GetSelectedProject<IVsHierarchy>()?.GetDTEProject();
            var dacpacPath = Path.ChangeExtension(project.FullName, DacPacExtension);
            CreateDacPac(settings, sprocFiles, dacpacPath);
        }

        /// <summary>
        /// Packages stored procedures into a DACPAC and then publishes it to the database.
        /// </summary>
        private void PublishToDatabase(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles) {
            var project = _pss.GetSelectedProject<IVsHierarchy>();
            var dacpacPath = Path.ChangeExtension(Path.GetTempFileName(), DacPacExtension);

            CreateDacPac(settings, sprocFiles, dacpacPath);
            var package = _dacServices.Load(dacpacPath);
            var dbName = settings.TargetDatabaseConnection.GetValue(ConnectionStringConverter.OdbcDatabaseKey);
            var connection = settings.TargetDatabaseConnection.OdbcToSqlClient();
            _dacServices.Deploy(package, connection, dbName);
        }

        /// <summary>
        /// Generates SQL files for stored procedures as well as publishing scripts
        /// and then adds them to the target database project.
        /// </summary>
        private void PublishToProject(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles) {
            Check.ArgumentNull(nameof(settings), settings.TargetProject);
            try {
                var targetProject = GetProjectByName(settings.TargetProject);
                var generator = new SProcProjectFilesGenerator(_pss, _fs);
                generator.Generate(settings, targetProject);
            } catch (Exception ex) {
                _appShell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_UnableGenerateSqlFiles, ex.Message));
                GeneralLog.Write(ex);
                if (ex.IsCriticalException()) {
                    throw ex;
                }
            }
        }

        private void CreateDacPac(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles, string dacpacPath) {
            var project = _pss.GetSelectedProject<IVsHierarchy>()?.GetDTEProject();
            var g = new SProcScriptGenerator(_fs);
            var scripts = g.CreateStoredProcedureScripts(settings, sprocFiles);
            var builder = _dacServices.GetBuilder(_appShell);
            builder.Build(dacpacPath, project.Name, scripts.Values);
        }

        private EnvDTE.Project GetProjectByName(string projectName) {
            var projects = _pss.GetSolution().Projects;
            foreach (EnvDTE.Project p in projects) {
                if (p.Name.EqualsOrdinal(projectName)) {
                    return p;
                }
            }
            return null;
        }
    }
}
