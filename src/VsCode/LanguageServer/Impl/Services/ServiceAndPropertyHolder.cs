// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Services {
    /// <summary>
    /// Base class for objects that have properties and services
    /// such as <see cref="IEditorBuffer" /> and <see cref="IEditorView"/>.
    /// </summary>
    internal abstract class ServiceAndPropertyHolder : IPropertyHolder {
        private readonly Lazy<PropertyDictionary> _properties = Lazy.Create(() => new PropertyDictionary());
        protected ServiceManager ServiceManager { get; } = new ServiceManager();

        public PropertyDictionary Properties => _properties.Value;
        public IServiceManager Services => ServiceManager;
    }
}
