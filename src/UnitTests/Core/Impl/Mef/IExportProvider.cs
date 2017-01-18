// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.UnitTests.Core.Mef {
    public interface IExportProvider : IMethodFixture {
        T GetExportedValue<T>();
        T GetExportedValue<T>(string metadataKey, params object[] metadataValues);
        IEnumerable<Lazy<T>> GetExports<T>();
        IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>();
    }
}