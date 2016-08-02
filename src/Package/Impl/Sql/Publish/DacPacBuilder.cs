// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Shell;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.R.Package.Logging;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    // 
    /// <summary>
    /// Data-tier application package builder.
    /// </summary>
    /// <remarks>
    /// Based on Based on https://github.com/Microsoft/DACExtensions/blob/master/SampleConsoleApp/ModelEndToEnd.cs
    /// </remarks>
    internal sealed class DacPacBuilder: IDacPacBuilder {
        private readonly OutputWindowLogWriter _outputWindow;
        private readonly ICoreShell _coreShell;

        public DacPacBuilder(ICoreShell coreShell) {
            _coreShell = coreShell;
            _outputWindow = new OutputWindowLogWriter(VSConstants.OutputWindowPaneGuid.BuildOutputPane_guid, string.Empty);
        }

        public void Build(string dacpacPath, string packageName, IEnumerable<string> scripts) {
            using (var model = new TSqlModel(SqlServerVersion.Sql110, new TSqlModelOptions { })) {
                // Adding objects to the model. 
                foreach (string script in scripts) {
                    model.AddObjects(script);
                }
                try {
                    // save the model to a new .dacpac
                    // Note that the PackageOptions can be used to specify RefactorLog and contributors to include
                    DacPackageExtensions.BuildPackage(dacpacPath, model,
                                        new PackageMetadata { Name = packageName, Description = string.Empty, Version = "1.0" },
                                        new PackageOptions());
                    var message = Environment.NewLine + 
                        string.Format(CultureInfo.InvariantCulture, Resources.SqlPublish_PublishDacpacSuccess, dacpacPath) +
                        Environment.NewLine;
                    _outputWindow.WriteAsync(MessageCategory.General, message).DoNotWait();
                } catch(DacServicesException ex) {
                    var error = Environment.NewLine + 
                        string.Format(CultureInfo.InvariantCulture, Resources.SqlPublishDialog_UnableToBuildDacPac, ex.Message) +
                        Environment.NewLine;
                    _outputWindow.WriteAsync(MessageCategory.Error, error).DoNotWait();
                    // _coreShell.ShowErrorMessage(error);
                }
            }
        }
    }
}
