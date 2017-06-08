// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Host.Client {
    public class RContext : IRContext {
        protected bool Equals(RContext other) => other != null && CallFlag == other.CallFlag;

        public RContext(RContextType callFlag) {
            CallFlag = callFlag;
        }

        public RContextType CallFlag { get; }

        public override bool Equals(object obj) => Equals(obj as RContext);

        public override int GetHashCode() => (int)CallFlag;
    }
}