// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using Microsoft.Common.Core.Exceptions;
using static System.FormattableString;

namespace Microsoft.Common.Core.Disposables {
    public sealed class DisposeToken {
        private readonly Func<Exception> _exceptionFactory;
        private int _disposed;

        public static DisposeToken Create<T>() where T : IDisposable {
            return new DisposeToken(GetObjectDisposedException<T>);
        }

        public static DisposeToken Create<T>(T instance) where T : IDisposable {
            return instance != null ? new DisposeToken(() => GetInstanceDisposedException(instance)) : Create<T>();
        }

        private DisposeToken(Func<Exception> exceptionFactory) {
            _exceptionFactory = exceptionFactory;
        }

        public bool IsDisposed => _disposed == 1;

        public void ThrowIfDisposed() {
            if (_disposed == 0) {
                return;
            }

            throw _exceptionFactory();
        }

        public bool TryMarkDisposed() {
            return Interlocked.Exchange(ref _disposed, 1) == 0;
        }

        private static ObjectDisposedException GetObjectDisposedException<T>() => new ObjectDisposedException(typeof(T).Name, Invariant($"{typeof(T).Name} instance is disposed"));
        private static ObjectDisposedException GetInstanceDisposedException<T>(T instance) => new InstanceDisposedException<T>(instance, Invariant($"{typeof(T).Name} instance is disposed"));
    }
}