// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Editor.Completion {
    /// <summary>
    /// Provides list of R packages loaded into the R workspace
    /// Exported via MEF.
    /// </summary>
    public interface ILoadedPackagesProvider {
        void Initialize();
        IEnumerable<string> GetPackageNames();
    }
}
