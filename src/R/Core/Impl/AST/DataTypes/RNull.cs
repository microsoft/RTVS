// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.AST.DataTypes.Definitions;

namespace Microsoft.R.Core.AST.DataTypes {
    public sealed class RNull : RObject, IRVector {
        private static readonly RNull _null = new RNull();
        public static RNull Null => _null;

        public int Length {
            get { return 0; }
        }

        public RMode Mode {
            get { return RMode.Null; }
        }
    }
}
