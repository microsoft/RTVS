// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.R.Support.Help.Functions {
    public sealed class FunctionInfo : NamedItemInfo, IFunctionInfo {
        #region IFunctionInfo
        /// <summary>
        /// Other function name variants
        /// </summary>
        public IReadOnlyList<string> Aliases { get; set; }

        /// <summary>
        /// Function signatures
        /// </summary>
        public IReadOnlyList<ISignatureInfo> Signatures { get; set; }

        /// <summary>
        /// Return value description
        /// </summary>
        public string ReturnValue { get; set; }

        /// <summary>
        /// Indicates that function is internal (has 'internal' 
        /// in its list of keywords)
        /// </summary>
        public bool IsInternal { get; internal set; }
        #endregion

        public FunctionInfo(string name, string description) :
            base(name, description, NamedItemType.Function) {
        }

        public FunctionInfo(string name) :
            base(name, NamedItemType.Function) {
        }
    }
}
