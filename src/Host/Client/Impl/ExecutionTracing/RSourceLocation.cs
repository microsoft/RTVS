// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using static System.FormattableString;

namespace Microsoft.R.ExecutionTracing {
    /// <summary>
    /// A location in the source code, identified by a file name and a line number.
    /// </summary>
    public struct RSourceLocation : IEquatable<RSourceLocation> {
        public string FileName { get; }
        public int LineNumber { get; }

        public RSourceLocation(string fileName, int lineNumber) {
            FileName = fileName;
            LineNumber = lineNumber;
        }

        public override int GetHashCode() =>
            new { FileName, LineNumber }.GetHashCode();

        public override bool Equals(object obj) =>
            (obj as RSourceLocation?)?.Equals(this) ?? false;

        public bool Equals(RSourceLocation other) =>
            FileName == other.FileName && LineNumber == other.LineNumber;
 
        public override string ToString() =>
            Invariant($"{FileName}:{LineNumber}");
    }
}
