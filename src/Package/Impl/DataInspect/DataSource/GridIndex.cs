// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.R.Core.AST.Operators;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public struct GridIndex : IEquatable<GridIndex> {
        public long Column { get; }
        public long Row { get; }

        public GridIndex(long row, long column) {
            Column = column;
            Row = row;
        }

        public static bool operator ==(GridIndex index1, GridIndex index2) => index1.Equals(index2);

        public static bool operator !=(GridIndex index1, GridIndex index2) => !index1.Equals(index2);

        public bool Equals(GridIndex other) => Column == other.Column && Row == other.Row;

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is GridIndex && Equals((GridIndex) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (Column.GetHashCode() * 397) ^ Row.GetHashCode();
            }
        }
    }
}