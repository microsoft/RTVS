// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.R.Core.AST.Scopes {
    [DebuggerDisplay("Global Scope, Children: {Children.Count} [{Start}...{End})")]
    public sealed class GlobalScope : Scope {
        public GlobalScope() :
            base("Global") {
        }

        #region ITextRange
        public override int Start => 0;

        public override int End {
            get {
                if (Root != null && Root.TextProvider != null) {
                    return Root.TextProvider.Length;
                }

                return base.End;
            }
        }

        public override bool Contains(int position) {
            return position >= Start && position <= End;
        }
        #endregion

        #region IAstNode
        public override IAstNode NodeFromPosition(int position) {
            var node = base.NodeFromPosition(position);
            return node ?? this;
        }

        public override IAstNode NodeFromRange(ITextRange range, bool inclusiveEnd = false) {
            var node = base.NodeFromRange(range, inclusiveEnd);
            return node ?? this;
        }
        #endregion
    }
}
