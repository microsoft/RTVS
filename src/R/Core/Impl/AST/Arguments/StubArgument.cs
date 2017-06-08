// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.R.Core.AST.Arguments {
    /// <summary>
    /// Represents missing argument in an incomplete function
    /// such as in 'func(' or 'func(,,'. 
    /// </summary>
    [DebuggerDisplay("Stub Argument")]
    public sealed class StubArgument : MissingArgument {
        // Stub argument does not have real position in the
        // buffer as there is no corresponding token, but we 
        // have to give it some position so it can be included
        // the text range collection.
        //  
        #region ITextRange
        public override int Start => int.MaxValue;
        public override int End => int.MaxValue;

        public override void Shift(int offset) { }

        public override void ShiftStartingFrom(int position, int offset) { }
        #endregion

        public override string ToString() => "{Stub}";
    }
}
