// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Services {
    /// <summary>
    /// Represents object that is a wrapper over platform-specific implementation
    /// and the implementation can be accessed in the platform-specific code.
    /// </summary>
    public interface IPlatformSpecificObject {
        /// <summary>
        /// Retrieves underlying platform-specific implementation
        /// </summary>
        /// <typeparam name="T">Platform-specific undelying object type</typeparam>
        T As<T>() where T : class;
    }
}
