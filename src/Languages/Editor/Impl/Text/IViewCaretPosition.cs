// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Languages.Editor.Text {
    public interface IViewCaretPosition: ISnapshotPoint {
        int VirtualSpaces { get; }
    }
}
