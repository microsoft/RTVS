// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Languages.Editor.Composition {
    // These interfaces are used as MEF metadata when importing objects

    public interface IComponentContentTypes {
        [DefaultValue(null)]
        IEnumerable<string> ContentTypes { get; }
    }

    public interface IOrderedComponentContentTypes : IComponentContentTypes, IOrderable { }
}
