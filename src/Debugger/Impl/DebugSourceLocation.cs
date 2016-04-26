// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using static System.FormattableString;

namespace Microsoft.R.Debugger {
    /// <summary>
    /// A location in the source code, identified by a file name and a line number.
    /// </summary>
    public struct DebugSourceLocation : IEquatable<DebugSourceLocation> {
        public string FileName { get; }
        public int LineNumber { get; }

        public DebugSourceLocation(string fileName, int lineNumber) {
            FileName = fileName;
            LineNumber = lineNumber;
        }

        public override int GetHashCode() {
            return new { FileName, LineNumber }.GetHashCode();
        }

        public override bool Equals(object obj) {
            return (obj as DebugSourceLocation?)?.Equals(this) ?? false;
        }

        public bool Equals(DebugSourceLocation other) {
            return FileName == other.FileName && LineNumber == other.LineNumber;
        }

        public override string ToString() {
            return Invariant($"{FileName}:{LineNumber}");
        }
    }
}
