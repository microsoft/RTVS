// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Disposables;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Disposables {
    [ExcludeFromCodeCoverage]
    public class DisposableTest {
        [Test]
        public void Create() {
            var callCount = 0;
            Action callback = () => callCount++;

            var disposable = Disposable.Create(callback);
            callCount.Should().Be(0);

            disposable.Dispose();
            callCount.Should().Be(1);

            disposable.Dispose();
            callCount.Should().Be(1);
        }
    }
}
