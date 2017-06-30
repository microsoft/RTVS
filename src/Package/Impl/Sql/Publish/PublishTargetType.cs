// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    public enum PublishTargetType {
        /// <summary>
        /// Publish to a Data-tier Application Component Packages (DACPAC) 
        /// file for subsequent import into a database
        /// </summary>
        Dacpac,

        /// <summary>
        /// Publish directly to a database
        /// </summary>
        Database,

        /// <summary>
        /// Publish generated files into an existing database project
        /// </summary>
        Project,

        /// <summary>
        /// Add generated file to the current project
        /// </summary>
        File
    }
}
