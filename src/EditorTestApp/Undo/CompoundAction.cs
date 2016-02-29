// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Undo;

namespace Microsoft.Languages.Editor.Application.Undo
{
    [ExcludeFromCodeCoverage]
    internal class CompoundAction : ICompoundUndoAction
    {
        public void Open(string name)
        {
        }

        public void Close(bool discardChanges)
        {
        }
    }
}
