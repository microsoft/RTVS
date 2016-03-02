// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Components.Test.Fakes.Undo {
    /// <summary>
    /// This is the delegate that we ultimately call to perform the work of the
    /// delegated undo operations. It contains information about all the parameter
    /// objects as well as the history of origin.
    /// </summary>
    internal delegate void UndoableOperationCurried();
}
