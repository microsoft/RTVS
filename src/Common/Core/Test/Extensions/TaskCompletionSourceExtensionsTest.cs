// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using FluentAssertions;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Common.Core.Test.Extensions {
    [ExcludeFromCodeCoverage]
    [Category.CoreExtensions]
    public class TaskCompletionSourceExtensionsTest {
        [Test]
        public async Task CanceledOnToken() {
            var tcs = new TaskCompletionSource<int>();
            var cts = new CancellationTokenSource();
            tcs.RegisterForCancellation(cts.Token);
            cts.CancelAfter(200);
            await ParallelTools.When(tcs.Task);
            tcs.Task.Should().BeCanceled();
        }

        [Test]
        public async Task CanceledOnTimeout() {
            var tcs = new TaskCompletionSource<int>();
            tcs.RegisterForCancellation(200, CancellationToken.None);
            await ParallelTools.When(tcs.Task);
            tcs.Task.Should().BeCanceled();
        }
    }
}