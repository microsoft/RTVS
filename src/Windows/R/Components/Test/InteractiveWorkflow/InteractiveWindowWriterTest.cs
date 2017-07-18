// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Testing;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.InteractiveWindow;
using NSubstitute;

namespace Microsoft.R.Components.Test.InteractiveWorkflow {
    [ExcludeFromCodeCoverage]
    [Category.Repl]
    public sealed class InteractiveWindowWriterTest: IDisposable {
        private readonly InteractiveWindowWriter _writer;
        private readonly IInteractiveWindow _interactiveWindow;

        public InteractiveWindowWriterTest(IServiceContainer services) {
            _interactiveWindow = Substitute.For<IInteractiveWindow>();
            _writer = new InteractiveWindowWriter(services.MainThread(), _interactiveWindow);
        }

        public void Dispose() {
            _writer?.Dispose();
        }

        [Test]
        public void MessageQueueTest01() {
            using (var mq = new InteractiveWindowWriter.MessageQueue()) {
                IEnumerable<InteractiveWindowWriter.Message> messages = null;

                var t = Task.Run(async () => {
                    messages = await mq.WaitForMessagesAsync();
                });

                t.Wait(100);
                messages.Should().BeNull();

                mq.Enqueue("test", false);
                t.Wait();
                messages.Should().ContainSingle(m => m.Text == "test" && m.IsError == false);
            }
        }

        [Test]
        public async Task MessageQueueTest02() {
            using (var mq = new InteractiveWindowWriter.MessageQueue()) {
                mq.Enqueue("start1", false);
                mq.Enqueue("start2\n", false);
                mq.Enqueue("\rtest1", false);
                mq.Enqueue(" test2", false);
                mq.Enqueue("test3\n", false);
                mq.Enqueue("\rtest4", false);
                mq.Enqueue("test5", false);
                mq.Enqueue("test6", false);
                mq.Enqueue("\rtest4", false);
                mq.Enqueue(" test8", false);

                var messages = (await mq.WaitForMessagesAsync()).Select(m => m.Text);
                messages.Should().ContainInOrder(
                    "start1", "start2\n", "\rtest1 test2", "test3\n", "\rtest4 test8"
                );
            }
        }
    }
}
