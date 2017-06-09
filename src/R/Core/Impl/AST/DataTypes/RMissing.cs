// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Core.AST.DataTypes.Definitions;

namespace Microsoft.R.Core.AST.DataTypes {
    public sealed class RMissing : RObject, IRVector {
        public static RMissing NA = new RMissing();

        public int Length => 0;
        public RMode Mode => RMode.Logical;
    }
}
