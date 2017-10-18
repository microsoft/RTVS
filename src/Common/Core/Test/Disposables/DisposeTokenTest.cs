// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Microsoft.Common.Core.Disposables;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Disposables {
    [ExcludeFromCodeCoverage]
    public class DisposeTokenTest : IDisposable {
        private readonly DisposeToken _disposeToken;

        public DisposeTokenTest() {
            _disposeToken = DisposeToken.Create<DisposeTokenTest>();
        }

        [Test]
        public void TryMarkDisposed() {
            _disposeToken.IsDisposed.Should().BeFalse();
            _disposeToken.TryMarkDisposed().Should().BeTrue();
            _disposeToken.IsDisposed.Should().BeTrue();
        }

        [Test]
        public void TryMarkDisposed_Concurrent() {
            var results = ParallelTools.Invoke(50, i => _disposeToken.TryMarkDisposed());

            results.Should().ContainSingle(r => r);
        }

        [Test]
        public void ThrowIfDisposed_Disposed() {
            _disposeToken.TryMarkDisposed();

            Action a = () => _disposeToken.ThrowIfDisposed();
            a.ShouldThrow<ObjectDisposedException>();
        }

        [Test]
        public void ThrowIfDisposed_NotDisposed() {
            Action a = () => _disposeToken.ThrowIfDisposed();
            a.ShouldNotThrow<ObjectDisposedException>();
        }

        [Test]
        public void CancelLinked() {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            using (_disposeToken.Link(ref token)) { 
                _disposeToken.TryMarkDisposed();
            }

            token.IsCancellationRequested.Should().BeTrue();
            cts.Token.IsCancellationRequested.Should().BeFalse();
        }

        [Test]
        public void CancelLinked_DefaultCancellationToken() {
            var token = default(CancellationToken);
            using (_disposeToken.Link(ref token)) { 
                _disposeToken.TryMarkDisposed();
            }

            token.IsCancellationRequested.Should().BeTrue();
        }

        [Test]
        public void LinkToDisposed() {
            _disposeToken.TryMarkDisposed();
            var token = default(CancellationToken);
            Action a = () => _disposeToken.Link(ref token);
            a.ShouldThrow<OperationCanceledException>();
        }

        public void Dispose() {
            _disposeToken.TryMarkDisposed();
        }
    }
}
