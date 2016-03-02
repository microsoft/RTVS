// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.R.Core.AST.DataTypes {
    [DebuggerDisplay("[{ClassName}, {Length}]")]
    public sealed class RClass : RList {
        public string ClassName {
            get {

                RString rs = this[new RString("class")] as RString;
                if (rs != null) {
                    return rs.Value;
                }

                return string.Empty;
            }
        }

        public RClass(string className) {
            this[new RString("class")] = new RString(className);
        }
    }
}
