// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Threading;

namespace Microsoft.R.Host.Client {
    public partial class RHostSession {
        private class NullLog : IActionLog {
            public LogVerbosity LogVerbosity => LogVerbosity.None;
            public void Flush() { }
            public void Write(LogVerbosity verbosity, MessageCategory category, string message) { }
            public void WriteFormat(LogVerbosity verbosity, MessageCategory category, string format, params object[] arguments) { }
            public void WriteLine(LogVerbosity verbosity, MessageCategory category, string message) { }
            public string Folder => Path.GetTempPath();
        }

        private sealed class NullLock : IExclusiveReaderLock {
            public Task<IAsyncReaderWriterLockToken> WaitAsync(CancellationToken cancellationToken = default(CancellationToken))
                => Task.FromResult<IAsyncReaderWriterLockToken>(new NullToken());
        }

        private sealed class NullToken : IAsyncReaderWriterLockToken {
            public ReentrancyToken Reentrancy => default(ReentrancyToken);
            public void Dispose() { }
        }
    }
}
