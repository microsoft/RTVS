// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Languages.Core.Composition {
    public interface IContentTypeMetadata {
        IEnumerable<string> ContentTypes { get; }
    }
}
