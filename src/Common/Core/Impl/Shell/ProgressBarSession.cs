// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.Common.Core.Shell {
    public class ProgressBarSession : IDisposable {
        private readonly IDisposable _disposable;
        public CancellationToken UserCancellationToken { get; }

        public ProgressBarSession(IDisposable disposable = null, CancellationToken cancellationToken = default(CancellationToken)) {
            _disposable = disposable;
            UserCancellationToken = cancellationToken;
        }

        public void Dispose() {
            _disposable?.Dispose();
        }
    }

    public class ProgressBarSession<T> : ProgressBarSession {
        public ProgressBarSession(IDisposable disposable = null, CancellationToken cancellationToken = default(CancellationToken), IProgress<T> progress = null) :
            base (disposable, cancellationToken){
            Progress = progress;
        }

        public IProgress<T> Progress { get; }
    }

}