// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Sql.Publish {
    /// <summary>
    /// Represents DAC package
    /// </summary>
    public interface IDacPackage {
        void Deploy(string connectionString, string databaseName);
    }
}
