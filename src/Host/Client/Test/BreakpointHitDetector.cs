// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.ExecutionTracing;

namespace Microsoft.R.Host.Client.Test {
    public class BreakpointHitDetector {
        public IRBreakpoint Breakpoint { get; }
        public bool WasHit { get; private set; }

        public BreakpointHitDetector(IRBreakpoint bp) {
            Breakpoint = bp;
            Breakpoint.BreakpointHit += Breakpoint_BreakpointHit;
        }

        public void Reset() {
            WasHit = false;
        }

        private void Breakpoint_BreakpointHit(object sender, EventArgs e) {
            WasHit = true;
            Breakpoint.BreakpointHit -= Breakpoint_BreakpointHit;
        }

        public async Task ShouldBeHitAtNextPromptAsync() {
            await Breakpoint.Tracer.Session.NextPromptShouldBeBrowseAsync();
            Breakpoint.BreakpointHit -= Breakpoint_BreakpointHit;
            WasHit.Should().BeTrue("Breakpoint must be hit at the next prompt.");
        }
    }
}
