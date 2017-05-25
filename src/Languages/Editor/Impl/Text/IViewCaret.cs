// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Languages.Editor.Text {
    public interface IViewCaret {
        bool InVirtualSpace { get; }
        IViewCaretPosition Position { get; }

        event EventHandler PositionChanged;
        void MoveTo(int point, int virtualSpaces);
    }
}
