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
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.R.Components.Sql;
using Microsoft.R.Components.Sql.Publish;
using Microsoft.VisualStudio.R.Package.Logging;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    /// <summary>
    /// Provides functionality for publishing stored procedures to projects or databases
    /// </summary>
    internal sealed class SProcPublisher {
        private const string DacPacExtension = "dacpac";

        private readonly OutputWindowLogWriter _outputWindow;
        private readonly IProjectSystemServices _pss;
        private readonly IServiceContainer _services;
        private readonly IDacPackageServices _dacServices;

        public SProcPublisher(IServiceContainer services, IProjectSystemServices pss, IDacPackageServices dacServices) {
            _services = services;
            _pss = pss;
            _dacServices = dacServices;
            _outputWindow = new OutputWindowLogWriter(services, VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, string.Empty);
        }

        public void Publish(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles) {
            switch (settings.TargetType) {
                case PublishTargetType.Project:
                case PublishTargetType.File:
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
            if (project != null) {
                var dacpacPath = Path.ChangeExtension(project.FullName, DacPacExtension);
                CreateDacPac(settings, sprocFiles, dacpacPath);
                RtvsTelemetry.Current?.TelemetryService.ReportEvent(TelemetryArea.SQL,
                    SqlTelemetryEvents.SqlDacPacPublish);
            }
        }

        /// <summary>
        /// Packages stored procedures into a DACPAC and then publishes it to the database.
        /// </summary>
        private void PublishToDatabase(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles) {
            var dacpacPath = Path.ChangeExtension(Path.GetTempFileName(), DacPacExtension);

            CreateDacPac(settings, sprocFiles, dacpacPath);
            using (var package = _dacServices.Load(dacpacPath)) {

                var dbName = settings.TargetDatabaseConnection.GetValue(ConnectionStringConverter.OdbcDatabaseKey);
                var connection = settings.TargetDatabaseConnection.OdbcToSqlClient();
                package.Deploy(connection, dbName);

                var message = Environment.NewLine +
                    string.Format(CultureInfo.InvariantCulture, Resources.SqlPublish_PublishDatabaseSuccess, connection) +
                    Environment.NewLine;
                _outputWindow.WriteAsync(MessageCategory.General, message).DoNotWait();
                RtvsTelemetry.Current?.TelemetryService.ReportEvent(TelemetryArea.SQL, SqlTelemetryEvents.SqlDatabasePublish);
            }
        }

        /// <summary>
        /// Generates SQL files for stored procedures as well as publishing scripts
        /// and then adds them to the target database project.
        /// </summary>
        private void PublishToProject(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles) {
            Check.ArgumentNull(nameof(settings), settings.TargetProject);
            var targetProject = _pss.GetProject(settings.TargetProject);
            var generator = new SProcProjectFilesGenerator(_pss, _services.FileSystem());
            targetProject = targetProject ?? _pss.GetActiveProject();
            generator.Generate(settings, sprocFiles, targetProject);
            RtvsTelemetry.Current?.TelemetryService.ReportEvent(TelemetryArea.SQL, SqlTelemetryEvents.SqlProjectPublish);
        }

        private void CreateDacPac(SqlSProcPublishSettings settings, IEnumerable<string> sprocFiles, string dacpacPath) {
            var project = _pss.GetSelectedProject<IVsHierarchy>()?.GetDTEProject();
            if (project != null) {
                var g = new SProcScriptGenerator(_services.FileSystem());
                var sprocMap = g.CreateStoredProcedureScripts(settings, sprocFiles);
                var builder = _dacServices.GetBuilder();
                builder.Build(dacpacPath, project.Name, sprocMap.Scripts);
            }
        }
    }
}
