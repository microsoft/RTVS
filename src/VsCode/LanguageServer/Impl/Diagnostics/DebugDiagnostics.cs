// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.R.LanguageServer.Diagnostics {
    internal sealed class DebugMeasureTime : IDisposable {
        private readonly DateTime _start = DateTime.Now;
        private readonly string _message;

        public DebugMeasureTime(string message) {
            _message = message;
        }

        public void Dispose() { }
        //=> Debug.WriteLine($"{_message}: " + (DateTime.Now - _start).TotalMilliseconds);
    }
}
