// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Common.Core.Threading {
    public partial class AsyncReaderWriterLock {
        private class Token : IAsyncReaderWriterLockToken {
            private LockSource _lockSource;
            public ReentrancyToken Reentrancy { get; }

            public Token(LockSource lockSource) {
                _lockSource = lockSource;
                Reentrancy = LockTokenFactory.Create(lockSource);
            }

            public void Dispose() {
                if (_lockSource.TryRemoveFromQueue()) {
                    _lockSource = null;
                }
            }
        }
    }
}