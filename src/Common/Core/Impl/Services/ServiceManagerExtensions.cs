// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Services {
    public static class ServiceManagerExtensions {
        public static IServiceManager AddService<TService>(this IServiceManager services) 
            where TService : class => services.AddService<TService, TService>();
    }
}