// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Session {
    internal class NullRSessionProviderCallback : IRSessionProviderCallback {
        public Task<IntPtr> GetApplicationWindowHandleAsync() => Task.FromResult(IntPtr.Zero);
        public void WriteConsole(string text) {}
    }
}