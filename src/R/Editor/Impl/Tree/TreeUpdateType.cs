// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.R.Editor.Tree
{
    /// <summary>
    /// Describes changes in the AST
    /// </summary>
    public enum TreeUpdateType
    {
        PositionsOnly,
        NodesRemoved,
        //ScopeChanged,
        NewTree
    }
}
