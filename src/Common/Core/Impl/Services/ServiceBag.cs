// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Common.Core.Services {
    public class ServiceBag: IServiceBag {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static ServiceBag Create(object service) => new ServiceBag().Add(service);

        public ServiceBag() { }
        public ServiceBag(params object[] services) {
            foreach (var s in services) {
                Add(s);
            }
        }

        public virtual ServiceBag Add(object service) {
             _services[service.GetType()] = service;
            return this;
        }

        public T GetService<T>() where T: class  => GetService(typeof(T)) as T;

        public object GetService(Type serviceType) {
            object service;
            _services.TryGetValue(serviceType, out service);
            return service;
        }
    }
}