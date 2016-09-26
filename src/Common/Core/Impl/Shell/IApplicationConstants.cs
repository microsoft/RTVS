// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Shell {
    /// <summary>
    /// Defines application constants such as locale, registry key names, etc.
    /// Implemented by the host application. Imported via MEF.
    /// </summary>
    public interface IApplicationConstants {
        /// <summary>
        /// Application locale ID (LCID)
        /// </summary>
        int LocaleId { get; }

        string LocalMachineHive { get; }
    }
}
