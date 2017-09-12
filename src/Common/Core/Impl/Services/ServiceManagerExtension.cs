// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Common.Core.Services {
    internal sealed class ServiceManagerExtension : ServiceManager {
        private readonly IServiceContainer _parent;

        public override IEnumerable<Type> AllServices => _parent.AllServices.Union(base.AllServices);

        public ServiceManagerExtension(IServiceContainer parent) {
            _parent = parent;
        }

        public override T GetService<T>(Type type = null) {
            return base.GetService<T>(type) ?? _parent.GetService<T>(type);
        }
    }
}