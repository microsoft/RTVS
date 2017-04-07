// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.UnitTests.Core.Mef {
    public interface IExportProvider {
        T GetExportedValue<T>() where T : class;
        T GetExportedValue<T>(string metadataKey, params object[] metadataValues) where T : class;
        IEnumerable<Lazy<T>> GetExports<T>() where T : class;
        IEnumerable<Lazy<T, TMetadataView>> GetExports<T, TMetadataView>() where T : class;
    }
}