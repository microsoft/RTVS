// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.VisualStudio.R.Package.Editors {
    /// <summary>
    /// Abstracts creation of VS object instances.
    /// Used primarity in unit tests so tests can provide
    /// different factory that creates mocks since 
    /// Package.CreateInstance cannot be overridden.
    /// </summary>
    internal interface IObjectInstanceFactory {
        object CreateInstance(ref Guid clsid, ref Guid riid, Type objectType);
    }
}
