// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Threading;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal class VsMainThreadAwaiter : IMainThreadAwaiter {
        private readonly JoinableTaskFactory.MainThreadAwaiter _awaiter;

        public VsMainThreadAwaiter(JoinableTaskFactory.MainThreadAwaiter awaiter) {
            _awaiter = awaiter;
        }

        public bool IsCompleted => _awaiter.IsCompleted;
        public void OnCompleted(Action continuation) => _awaiter.OnCompleted(continuation);
        public void GetResult() => _awaiter.GetResult();
    }
}