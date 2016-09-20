// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Sql {
    /// <summary>
    /// Represents various database connectivity services. Exported via MEF.
    /// </summary>
    public interface IDbConnectionService {
        string EditConnectionString(string connectionString);
    }
}
