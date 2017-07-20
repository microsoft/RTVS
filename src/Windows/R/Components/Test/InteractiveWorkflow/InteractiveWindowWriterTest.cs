// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.InteractiveWindow;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Components.Test.InteractiveWorkflow {
    [ExcludeFromCodeCoverage]
    [Category.Repl]
    public sealed class InteractiveWindowWriterTest: IDisposable {
        private readonly InteractiveWindowWriter _writer;

        public InteractiveWindowWriterTest(IServiceContainer services) {
            _writer = new InteractiveWindowWriter(services.MainThread(), Substitute.For<IInteractiveWindow>());
        }

        public void Dispose() => _writer?.Dispose();

        [Test]
        public void MessageQueueTest01() {
            using (var mq = new InteractiveWindowWriter.MessageQueue()) {
                IEnumerable<InteractiveWindowWriter.Message> messages = null;

                var t = Task.Run(async () => {
                    messages = await mq.WaitForMessagesAsync(CancellationToken.None);
                });

                t.Wait(100);
                messages.Should().BeNull();

                mq.Enqueue("test", false);
                t.Wait();
                messages.Should().ContainSingle(m => m.Text == "test" && m.IsError == false);
            }
        }

        [CompositeTest]
        [InlineData(new[] { "start1", "start2\n", "\rtest1", " test2", "test3\n", "\rtest4", "test5", "test6", "\rtest4", " test8" },
                    new[] { "start1", "start2\n", "\rtest1 test2", "test3\n", "\rtest4 test8" })]
        [InlineData(new[] { "st\rart1", "start2", "\rtest1", " test2"},
                    new[] { "\rtest1 test2" })]
        [InlineData(new[] { "st\rart1", "st\nart2", "\rtest1", " test2" },
                    new[] { "st\rart1", "st\nart2", "\rtest1 test2" })]
        public async Task MessageQueueTest02(IEnumerable<string> input, IEnumerable<string> expected) {
            using (var mq = new InteractiveWindowWriter.MessageQueue()) {
                foreach (var s in input) {
                    mq.Enqueue(s, false);
                }
                var messages = (await mq.WaitForMessagesAsync(CancellationToken.None)).Select(m => m.Text);
                messages.Should().ContainInOrder(expected);
            }
        }
    }
}
