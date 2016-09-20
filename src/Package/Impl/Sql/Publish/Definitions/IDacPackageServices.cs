// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.SqlServer.Dac;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    internal interface IDacPackageServices {
        IDacPacBuilder GetBuilder(ICoreShell coreShell);
        DacPackage Load(string dacpacPath);
        void Deploy(DacPackage package, string connectionString, string databaseName);
    }
}
