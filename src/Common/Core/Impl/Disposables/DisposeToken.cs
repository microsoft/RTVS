// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using static System.FormattableString;

namespace Microsoft.Common.Core.Disposables {
    public sealed class DisposeToken {
        private readonly Action _dispose;
        private readonly string _message;
        private int _disposed;

        public static DisposeToken Create<T>(Action dispose = null) where T : IDisposable {
            return new DisposeToken(dispose, Invariant($"{typeof(T).Name} instance is disposed"));
        }

        public DisposeToken(Action dispose = null, string message = null) {
            _dispose = dispose;
            _message = message;
        }

        public void ThrowIfDisposed() {
            if (_disposed == 0) {
                return;
            }

            if (_message == null) {
                throw new InvalidOperationException();
            }

            throw new InvalidOperationException(_message);
        }

        public bool TryMarkDisposed() {
            var markedDisposed = Interlocked.Exchange(ref _disposed, 1) == 0;
            if (markedDisposed) {
                _dispose?.Invoke();
            }
            return markedDisposed;
        }
    }
}