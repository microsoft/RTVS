// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Services {
    public static class ServiceManagerExtensions {
        // TODO: Add support for constructors with parameters
        public static IServiceManager AddService<TService, TImplementation>(this IServiceManager services)
            where TService : class
            where TImplementation : class, TService, new()
            => services.AddService<TService>(s => new TImplementation());
    }
}