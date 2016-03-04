using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Collections;
using Microsoft.UnitTests.Core.FluentAssertions;

namespace Microsoft.R.Debugger.Test {
    internal sealed class DebugStackFramesAssertions : GenericCollectionAssertions<DebugStackFrame> {
        protected override string Context => "DebugSession.GetStackFramesAsync()";

        private readonly IEnumerable<DebugStackFrame> _frames;

        public DebugStackFramesAssertions(IEnumerable<DebugStackFrame> frames)
            : base(frames) {
            _frames = frames.Reverse();
        }

        public AndConstraint<GenericCollectionAssertions<DebugStackFrame>> BeAt(MatchDebugStackFrames matchFrames) {
            return this.HaveHead<DebugStackFrame, MatchDebugStackFrame>(matchFrames);
        }
    }
}
