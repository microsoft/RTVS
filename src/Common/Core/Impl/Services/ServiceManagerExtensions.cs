// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Services {
    public static class ServiceManagerExtensions {
        public static IServiceManager AddService<TService>(this IServiceManager services)
            where TService : class
            => AddService<TService, TService>(services);

        public static IServiceManager AddService<TService>(this IServiceManager services, TService instance)
            where TService : class
            => services.AddService(instance, typeof(TService));

        public static IServiceManager AddService<TService, TImplementation>(this IServiceManager services)
            where TService : class
            where TImplementation : class, TService
            => services.AddService<TService>(ServiceContainerExtensions.CreateInstance<TImplementation>);
    }
}