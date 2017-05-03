// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.Common.Core.Events {
    public class DirtyEventSource {
        private readonly object _source;
        private int _dirty;

        public event EventHandler Event;

        public DirtyEventSource(object source) {
            _source = source;
        }

        public void Reset() => Interlocked.Exchange(ref _dirty, 0);

        public void FireOnce() {
            if (Interlocked.Exchange(ref _dirty, 1) == 0) {
                Event?.Invoke(_source, new EventArgs());
            }
        }
    }
}