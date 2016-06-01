// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Install {
    public interface ISupportedRVersionList {
        /// <summary>
        /// Minimal supported R version major part such as 3 in 3.2
        /// </summary>
        int MinMajorVersion { get; }
        /// <summary>
        /// Minimal supported R version minor part such as 2 in 3.2
        /// </summary>
        int MinMinorVersion { get; }
        /// <summary>
        /// Maximal supported R version major part such as 4 in 4.5
        /// </summary>
        int MaxMajorVersion { get; }
        /// <summary>
        /// Maximal supported R version minor part such as 5 in 4.5
        /// </summary>
        int MaxMinorVersion { get; }

        /// <summary>
        /// Determines if given version falls into the supported range.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        bool IsCompatibleVersion(Version v);
    }
}
