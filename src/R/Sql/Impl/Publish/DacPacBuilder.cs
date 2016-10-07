// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.R.Components.Sql.Publish;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;

namespace Microsoft.VisualStudio.R.Sql.Publish {
    // 
    /// <summary>
    /// Data-tier application package builder.
    /// </summary>
    /// <remarks>
    /// Based on Based on https://github.com/Microsoft/DACExtensions/blob/master/SampleConsoleApp/ModelEndToEnd.cs
    /// </remarks>
    public sealed class DacPacBuilder: IDacPacBuilder {
        public void Build(string dacpacPath, string packageName, IEnumerable<string> scripts) {
            using (var model = new TSqlModel(SqlServerVersion.Sql130, new TSqlModelOptions { })) {
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
                } catch(DacServicesException ex) {
                    throw new SqlPublishException(ex.Message);
                }
            }
        }
    }
}
