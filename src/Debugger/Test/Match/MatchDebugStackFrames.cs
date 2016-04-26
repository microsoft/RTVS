// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Common.Core.Test.Match;

namespace Microsoft.R.Debugger.Test.Match {
    internal class MatchDebugStackFrames : IEnumerable<IEquatable<DebugStackFrame>> {
        private readonly List<IEquatable<DebugStackFrame>> _frames = new List<IEquatable<DebugStackFrame>>();

        public IEnumerator<IEquatable<DebugStackFrame>> GetEnumerator() {
            return _frames.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public void Add(IEquatable<DebugStackFrame> match) {
            _frames.Add(match);
        }

        public void Add(IEquatable<string> fileName, IEquatable<int> lineNumber, IEquatable<string> call, IEquatable<string> environmentName) {
            Add(new MatchMembers<DebugStackFrame>()
                .Matching(x => x.FileName, fileName)
                .Matching(x => x.LineNumber, lineNumber)
                .Matching(x => x.Call, call)
                .Matching(x => x.EnvironmentName, environmentName));
        }

        public void Add(IEquatable<string> fileName, IEquatable<int> lineNumber, IEquatable<string> call) {
            Add(fileName, lineNumber, call, MatchAny<string>.Instance);
        }

        public void Add(IEquatable<string> fileName, IEquatable<int> lineNumber) {
            Add(fileName, lineNumber, MatchAny<string>.Instance);
        }

        public void Add(SourceFile sourceFile, IEquatable<int> lineNumber, IEquatable<string> call, IEquatable<string> environmentName) {
            Add(sourceFile.FilePath, lineNumber, call, environmentName);
        }

        public void Add(SourceFile sourceFile, IEquatable<int> lineNumber, IEquatable<string> call) {
            Add(sourceFile.FilePath, lineNumber, call);
        }

        public void Add(SourceFile sourceFile, IEquatable<int> lineNumber) {
            Add(sourceFile.FilePath, lineNumber);
        }

        public void Add(DebugSourceLocation location, int offset, IEquatable<string> call) {
            Add(location.FileName, location.LineNumber + offset, call);
        }

        public void Add(DebugSourceLocation location, int offset = 0) {
            Add(location.FileName, location.LineNumber + offset, MatchAny<string>.Instance);
        }

        public void Add(DebugSourceLocation location, IEquatable<string> call) {
            Add(location.FileName, location.LineNumber, call);
        }
    }
}
