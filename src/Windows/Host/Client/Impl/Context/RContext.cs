// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    internal class RContext : IRContext {
        protected bool Equals(RContext other) {
            return other != null && CallFlag == other.CallFlag;
        }

        public RContext(RContextType callFlag) {
            CallFlag = callFlag;
        }

        public RContextType CallFlag { get; }

        public override bool Equals(object obj) {
            return Equals(obj as RContext);
        }

        public override int GetHashCode() {
            return (int)CallFlag;
        }
    }
}