// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Editor.Functions {
    public sealed class FunctionInfo : NamedItemInfo, IFunctionInfo {
        #region IFunctionInfo
        /// <summary>
        /// Package the function belongs to
        /// </summary>
        public string Package { get; }

        /// <summary>
        /// Function signatures
        /// </summary>
        public IReadOnlyList<ISignatureInfo> Signatures { get; set; } = new List<ISignatureInfo>();

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

        public FunctionInfo(string name, string package,  string description) :
            base(name, description, NamedItemType.Function) {
            Package = package;
        }

        public FunctionInfo(string name) : this(name, null, string.Empty) { }

        public FunctionInfo(string alias, IFunctionInfo primary) : 
            this(alias, primary .Package, primary.Description) {
            Signatures = primary.Signatures;
            ReturnValue = primary.ReturnValue;
            IsInternal = primary.IsInternal;
        }
    }
}
