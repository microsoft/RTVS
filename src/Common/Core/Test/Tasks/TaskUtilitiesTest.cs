// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Common.Core.Test.Tasks {
    public class TaskUtilitiesTest {
        [Test]
        public async Task CreateCanceled() {
            var exception = new CustomOperationCanceledException();
            var task = TaskUtilities.CreateCanceled<int>(exception);

            task.Status.Should().Be(TaskStatus.Canceled);
            task.IsCanceled.Should().Be(true);

            Func<Task<int>> f = async () => await task;
            await f.ShouldThrowAsync<CustomOperationCanceledException>();
        }

        private class CustomOperationCanceledException : OperationCanceledException { }
    }
}
