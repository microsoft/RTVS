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
using Microsoft.Common.Core.Telemetry;
using Microsoft.R.Components.Sql;
using Microsoft.R.Components.Sql.Publish;
using Microsoft.VisualStudio.R.Package.Logging;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    /// <summary>
    /// Provides functionality for publishing stored procedures to projects or databases
    /// </summary>
    internal sealed class SProcPublisher {
        private const string DacPacExtension = "dacpac";

        private readonly OutputWindowLogWriter _outputWindow;
        private readonly IApplicationShell _appShell;
        private readonly IProjectSystemServices _pss;
        private readonly IFileSystem _fs;
        private readonly IDacPackageServices _dacServices;

        public SProcPublisher(IApplicationShell appShell, IProjectSystemServices pss, IFileSystem fs, IDacPackageServices dacServices) {
            _appShell = appShell;
            _pss = pss;
            _fs = fs;
            _dacServices = dacServices;
            _outputWindow = new OutputWindowLogWriter(VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, string.Empty);
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
            RtvsTelemetry.Current?.TelemetryService.ReportEvent(TelemetryArea.SQL, SqlTelemetryEvents.SqlDacPacPublish);
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
            package.Deploy(connection, dbName);

            var message = Environment.NewLine + 
                string.Format(CultureInfo.InvariantCulture, Resources.SqlPublish_PublishDatabaseSuccess, connection) + 
                Environment.NewLine;
            _outputWindow.WriteAsync(MessageCategory.General, message).DoNotWait();
            RtvsTelemetry.Current?.TelemetryService.ReportEvent(TelemetryArea.SQL, SqlTelemetryEvents.SqlDatabasePublish);
        }

        /// <summary>
        /// Generates SQL files for stored procedures as well as publishing scripts
        /// and then adds them to the target database project.
        /// </summary>
        private void PublishToProject(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles) {
            Check.ArgumentNull(nameof(settings), settings.TargetProject);
            var targetProject = _pss.GetProject(settings.TargetProject);
            var generator = new SProcProjectFilesGenerator(_pss, _fs);
            generator.Generate(settings, targetProject);
            RtvsTelemetry.Current?.TelemetryService.ReportEvent(TelemetryArea.SQL, SqlTelemetryEvents.SqlProjectPublish);
        }

        private void CreateDacPac(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles, string dacpacPath) {
            var project = _pss.GetSelectedProject<IVsHierarchy>()?.GetDTEProject();
            if (project != null) {
                var g = new SProcScriptGenerator(_fs);
                var sprocMap = g.CreateStoredProcedureScripts(settings, sprocFiles);
                var builder = _dacServices.GetBuilder();
                builder.Build(dacpacPath, project.Name, sprocMap.Scripts);
            }
        }
    }
}
