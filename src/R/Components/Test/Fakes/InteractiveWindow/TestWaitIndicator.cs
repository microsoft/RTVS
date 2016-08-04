// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense.Utilities;

namespace Microsoft.R.Components.Test.Fakes.InteractiveWindow {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IWaitIndicator))]
    internal sealed class TestWaitIndicator : IWaitIndicator {
        public IWaitContext StartWait(string title, string message, bool allowCancel) {
            return new WaitContext();
        }

        public WaitIndicatorResult Wait(string title, string message, bool allowCancel, Action<IWaitContext> action) {
            try {
                action(new WaitContext {Message = message});
            } catch (OperationCanceledException) {
                return WaitIndicatorResult.Canceled;
            }

            return WaitIndicatorResult.Completed;
        }

        private class WaitContext : IWaitContext {
            public bool AllowCancel { get; set; }
            public CancellationToken CancellationToken => CancellationToken.None;
            public string Message { get; set; }
            public void UpdateProgress() { }
            public void Dispose() { }
        }
    }
}