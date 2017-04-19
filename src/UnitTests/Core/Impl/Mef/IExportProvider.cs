// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.UnitTests.Core.Mef {
    public interface IExportProvider {
        T GetExportedValue<T>() where T : class;
    }
}