// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Services {
    public interface IServiceManager: IServiceContainer, IDisposable {
        /// <summary>
        /// Adds service instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="service">Service instance</param>
        IServiceManager AddService<T>(T service) where T : class;

        /// <summary>
        /// Adds on-demand created service
        /// </summary>
        /// <param name="factory">Optional service factory. If not provided, reflection with default constructor will be used.</param>
        IServiceManager AddService<T>(Func<T> factory = null);

        /// <summary>
        /// Removes service from container
        /// </summary>
        void RemoveService<T>() where T : class;
    }
}
