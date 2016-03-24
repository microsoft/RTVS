// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Debugger.Test {
    internal static class DebugSessionExtensions {
        public static async Task NextPromptShouldBeBrowseAsync(this DebugSession session) {
            // Grab the next available prompt, and verify that it's Browse>
            using (var inter = await session.RSession.BeginInteractionAsync()) {
                inter.Contexts.IsBrowser().Should().BeTrue("Next available prompt should be a Browse> prompt");
            }

            // Now wait until session tells us that it has noticed the prompt and processed it.
            // Note that even if we register the handler after it has already noticed, but
            // before the interaction completed, it will still invoke the handler.
            var browseTask = EventTaskSources.DebugSession.Browse.Create(session);

            // Spin until either Browse is raised, or we see a different non-Browse prompt.
            // If the latter happens, we're not going to see Browse raised for that prompt that we've
            // seen initially, because something had interfered asynchronously, which shouldn't happen
            // in a test - and if it does, the test should be considered failed at that point, because
            // the state is indeterminate.
            while (true) {
                var interTask = session.RSession.BeginInteractionAsync();
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

        public static Task<DebugBreakpoint> CreateBreakpointAsync(this DebugSession session, SourceFile sf, int lineNumber) {
            return session.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, lineNumber));
        }
    }
}
