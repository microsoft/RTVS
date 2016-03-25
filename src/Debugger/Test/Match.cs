// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using static System.FormattableString;

namespace Microsoft.R.Debugger.Test {
    internal class MatchAny<T> : IEquatable<T> {
        public static readonly MatchAny<T> Instance = new MatchAny<T>();

        public bool Equals(T other) {
            return true;
        }

        public override bool Equals(object obj) {
            return true;
        }

        public override int GetHashCode() {
            return 0;
        }

        public override string ToString() {
            return "<any>";
        }
    }

    internal class MatchNull<T> : IEquatable<T> {
        public static readonly MatchNull<T> Instance = new MatchNull<T>();

        public bool Equals(T other) {
            return other == null;
        }

        public override bool Equals(object other) {
            return other == null;
        }

        public override int GetHashCode() {
            return 0;
        }

        public override string ToString() {
            return "<null>";
        }
    }

    internal class MatchRange<T> : IEquatable<T> where T : IComparable<T> {
        private readonly T _from, _to;

        public MatchRange(T from, T to) {
            _from = from;
            _to = to;
        }

        public bool Equals(T other) =>
            other == null ? false : other.CompareTo(_from) >= 0 && other.CompareTo(_to) <= 0;

        public override bool Equals(object other) =>
            other is T ? Equals((T)other) : false;

        public override int GetHashCode() =>
            new { _from, _to }.GetHashCode();

        public override string ToString() =>
            Invariant($"[{_from} .. {_to}]");
    }

    internal class MatchDebugStackFrame : IEquatable<DebugStackFrame> {
        public IEquatable<string> FileName { get; }
        public IEquatable<int> LineNumber { get; }
        public IEquatable<string> Call { get; }

        public MatchDebugStackFrame(IEquatable<string> fileName, IEquatable<int> lineNumber, IEquatable<string> call) {
            FileName = fileName ?? MatchNull<string>.Instance;
            LineNumber = lineNumber ?? MatchNull<int>.Instance;
            Call = call ?? MatchNull<string>.Instance;
        }

        public bool Equals(DebugStackFrame other) {
            return other != null &&
                FileName.Equals(other.FileName) &&
                LineNumber.Equals(other.LineNumber) &&
                Call.Equals(other.Call);
        }

        public override bool Equals(object other) {
            return Equals(other as DebugStackFrame);
        }

        public override int GetHashCode() {
            return new { FileName, LineNumber, Call }.GetHashCode();
        }

        public override string ToString() {
            return Invariant($"{Call} at {FileName}:{LineNumber}");
        }
    }

    internal class MatchDebugStackFrames : IEnumerable<MatchDebugStackFrame> {
        private readonly List<MatchDebugStackFrame> _frames = new List<MatchDebugStackFrame>();

        public IEnumerator<MatchDebugStackFrame> GetEnumerator() {
            return _frames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(IEquatable<string> fileName, IEquatable<int> lineNumber, IEquatable<string> call) {
            _frames.Add(new MatchDebugStackFrame(fileName, lineNumber, call));
        }

        public void Add(IEquatable<string> fileName, IEquatable<int> lineNumber) {
            Add(fileName, lineNumber, MatchAny<string>.Instance);
        }

        public void Add(SourceFile sourceFile, IEquatable<int> lineNumber, IEquatable<string> call) {
            Add(sourceFile.FilePath, lineNumber, call);
        }

        public void Add(SourceFile sourceFile, IEquatable<int> lineNumber) {
            Add(sourceFile.FilePath, lineNumber);
        }

        public void Add(DebugBreakpointLocation location, int offset, IEquatable<string> call) {
            Add(location.FileName, location.LineNumber + offset, call);
        }

        public void Add(DebugBreakpointLocation location, int offset = 0) {
            Add(location.FileName, location.LineNumber + offset, MatchAny<string>.Instance);
        }

        public void Add(DebugBreakpointLocation location, IEquatable<string> call) {
            Add(location.FileName, location.LineNumber, call);
        }
    }
}
