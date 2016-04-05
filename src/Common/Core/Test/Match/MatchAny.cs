// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Test.Match {
    public class MatchAny<T> : IEquatable<T> {
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
}
