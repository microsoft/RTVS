// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Microsoft.VisualStudio.Editor.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class PeekResultCollectionMock : List<IPeekResult>, IPeekResultCollection {
        public void Move(int oldIndex, int newIndex) {
            throw new NotImplementedException();
        }
    }
}
