// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Disposables;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Disposables {
    [ExcludeFromCodeCoverage]
    public class CountdownDisposableTest {
        [Test]
        public void Create() {
            var callCount = 0;
            Action callback = () => callCount++;

            var countdownDisposable = new CountdownDisposable(callback);
            countdownDisposable.Count.Should().Be(0);
            callCount.Should().Be(0);

            var disposable = countdownDisposable.Increment();
            countdownDisposable.Count.Should().Be(1);
            callCount.Should().Be(0);

            disposable.Dispose();
            countdownDisposable.Count.Should().Be(0);
            callCount.Should().Be(1);
        }

        [Test]
        public void Increment() {
            var countdownDisposable = new CountdownDisposable(() => { });
            countdownDisposable.Count.Should().Be(0);

            var disposable1 = countdownDisposable.Increment();
            countdownDisposable.Count.Should().Be(1);

            var disposable2 = countdownDisposable.Increment();
            countdownDisposable.Count.Should().Be(2);

            disposable2.Dispose();
            countdownDisposable.Count.Should().Be(1);

            disposable2.Dispose();
            countdownDisposable.Count.Should().Be(1);

            disposable2 = countdownDisposable.Increment();
            countdownDisposable.Count.Should().Be(2);

            disposable1.Dispose();
            countdownDisposable.Count.Should().Be(1);

            disposable2.Dispose();
            countdownDisposable.Count.Should().Be(0);
        }
    }
}