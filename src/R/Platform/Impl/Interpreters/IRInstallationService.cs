// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Platform.Interpreters {
    public interface IRInstallationService {
        IRInterpreterInfo CreateInfo(string name, string path);

        /// <summary>
        /// Retrieves path to the latest (highest version) R installation
        /// from registry. Typically in the form 'Program Files\R\R-3.2.1'
        /// Selects highest from compatible versions, not just the highest.
        /// </summary>
        IEnumerable<IRInterpreterInfo> GetCompatibleEngines(ISupportedRVersionRange svl = null);
    }
}