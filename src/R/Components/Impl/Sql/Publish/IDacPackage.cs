// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Sql.Publish {
    /// <summary>
    /// Represents DAC package
    /// </summary>
    public interface IDacPackage: IDisposable {
        void Deploy(string connectionString, string databaseName);
    }
}
