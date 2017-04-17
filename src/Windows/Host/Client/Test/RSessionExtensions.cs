// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.ExecutionTracing;
using Microsoft.R.StackTracing;

namespace Microsoft.R.Host.Client.Test {
    internal static class RSessionExtensions {
        public static async Task NextPromptShouldBeBrowseAsync(this IRSession session) {
            var tracer = await session.TraceExecutionAsync();      

            // Grab the next available prompt, and verify that it's Browse>
            using (var inter = await session.BeginInteractionAsync()) {
                inter.Contexts.IsBrowser().Should().BeTrue("Next available prompt should be a Browse> prompt");
            }

            // Now wait until session tells us that it has noticed the prompt and processed it.
            // Note that even if we register the handler after it has already noticed, but
            // before the interaction completed, it will still invoke the handler.
            var browseTask = EventTaskSources.IRExecutionTracer.Browse.Create(tracer);

            // Spin until either Browse is raised, or we see a different non-Browse prompt.
            // If the latter happens, we're not going to see Browse raised for that prompt that we've
            // seen initially, because something had interfered asynchronously, which shouldn't happen
            // in a test - and if it does, the test should be considered failed at that point, because
            // the state is indeterminate.
            while (true) {
                var interTask = session.BeginInteractionAsync();
                var completedTask = await Task.WhenAny(browseTask, interTask);

                if (completedTask == browseTask) {
                    interTask.ContinueWith(t => t.Result.Dispose(), TaskContinuationOptions.OnlyOnRanToCompletion).DoNotWait();
                    return;
                }

                using (var inter = await interTask) {
                    inter.Contexts.IsBrowser().Should().BeTrue();
                }
            }
        }

        public static async Task<IRStackFrame[]> ShouldHaveTracebackAsync(this IRSession session, TracebackBuilder builder) {
            var expected = builder.ToArray();
            var actual = (await session.TracebackAsync()).ToArray();
            actual.ShouldBeEquivalentTo(expected, options => builder.Configure(options));
            return actual;
        }

        public static async Task ShouldBeAtAsync(this IRSession session, string fileName, int lineNumber) {
            var actual = (await session.TracebackAsync()).Last();
            actual.FileName.Should().Be(fileName);
            actual.LineNumber.Should().Be(lineNumber);
        }

        public static Task ShouldBeAtAsync(this IRSession session, RSourceLocation location, int offset = 0) {
            return session.ShouldBeAtAsync(location.FileName, location.LineNumber + offset);
        }

        public static Task<IRBreakpoint> CreateBreakpointAsync(this IRExecutionTracer tracer, SourceFile sf, int lineNumber) {
            return tracer.CreateBreakpointAsync(new RSourceLocation(sf.FilePath, lineNumber));
        }
    }
}
