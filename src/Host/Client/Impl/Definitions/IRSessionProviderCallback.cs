// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSessionProviderCallback {
        Task<IntPtr> GetApplicationWindowHandleAsync();
        void WriteConsole(string text);
    }
}