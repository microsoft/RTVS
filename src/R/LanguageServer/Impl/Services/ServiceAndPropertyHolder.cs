// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;

namespace Microsoft.R.LanguageServer.Services {
    internal abstract class ServiceAndPropertyHolder : IPropertyHolder {
        private readonly Lazy<PropertyDictionary> _properties = Lazy.Create(() => new PropertyDictionary());

        public PropertyDictionary Properties => _properties.Value;
        public IServiceManager Services { get; } = new ServiceManager();
    }
}
