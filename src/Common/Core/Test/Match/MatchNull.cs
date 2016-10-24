// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Common.Core.Test.Match {
    [ExcludeFromCodeCoverage]
    public class MatchNull<T> : IEquatable<T> {
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
}
