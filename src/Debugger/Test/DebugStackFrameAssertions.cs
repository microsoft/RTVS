using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Collections;

namespace Microsoft.R.Debugger.Test {
    internal sealed class DebugStackFramesAssertions : GenericCollectionAssertions<DebugStackFrame> {
        protected override string Context => "DebugSession.GetStackFramesAsync()";

        private readonly IEnumerable<DebugStackFrame> _frames;

        public DebugStackFramesAssertions(IEnumerable<DebugStackFrame> frames)
            : base(frames) {
            _frames = frames.Reverse();
        }

        public RemainingFrames BeAt(string fileName, int? lineNumber) {
            return new RemainingFrames(_frames).At(fileName, lineNumber);
        }

        public RemainingFrames BeAt(string fileName, int? lineNumber, string call) {
            return new RemainingFrames(_frames).At(fileName, lineNumber, call);
        }

        public RemainingFrames BeAt(SourceFile sourceFile, int? lineNumber) {
            return new RemainingFrames(_frames).At(sourceFile, lineNumber);
        }

        public RemainingFrames BeAt(SourceFile sourceFile, int? lineNumber, string call) {
            return new RemainingFrames(_frames).At(sourceFile, lineNumber, call);
        }

        public RemainingFrames BeAt(DebugBreakpointLocation location, int delta = 0) {
            return new RemainingFrames(_frames).At(location, delta);
        }

        public RemainingFrames BeAt(DebugBreakpointLocation location, string call) {
            return new RemainingFrames(_frames).At(location, call);
        }

        public RemainingFrames BeAt(DebugBreakpointLocation location, int delta, string call) {
            return new RemainingFrames(_frames).At(location, delta, call);
        }

        public class RemainingFrames {
            private readonly IEnumerable<DebugStackFrame> _frames;

            public RemainingFrames(IEnumerable<DebugStackFrame> frames) {
                _frames = frames;
            }

            public RemainingFrames At(string fileName, int? lineNumber) {
                _frames.Should().NotBeEmpty();

                var frame = _frames.First();
                frame.FileName.Should().Be(fileName);
                frame.LineNumber.Should().Be(lineNumber);

                return new RemainingFrames(_frames.Skip(1));
            }

            public RemainingFrames At(string fileName, int? lineNumber, string call) {
                _frames.Should().HaveCount(n => n >= 2);
                var result = At(fileName, lineNumber);
                _frames.ElementAt(1).Call.Should().Be(call);
                return result;
            }

            public RemainingFrames At(SourceFile sourceFile, int? lineNumber) {
                return At(sourceFile.FilePath, lineNumber);
            }

            public RemainingFrames At(SourceFile sourceFile, int? lineNumber, string call) {
                return At(sourceFile.FilePath, lineNumber, call);
            }

            public RemainingFrames At(DebugBreakpointLocation location, int delta = 0) {
                return At(location.FileName, location.LineNumber + delta);
            }

            public RemainingFrames At(DebugBreakpointLocation location, string call) {
                return At(location.FileName, location.LineNumber, call);
            }

            public RemainingFrames At(DebugBreakpointLocation location, int delta, string call) {
                return At(location.FileName, location.LineNumber + delta, call);
            }
        }
    }
}
