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

        public bool IsDirty {
            get { return _dirty == 1; }
            set {
                if (Interlocked.Exchange(ref _dirty, value ? 1 : 0) == 0 && value) {
                    Event?.Invoke(_source, new EventArgs());
                }
            }
        }
    }
}