// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Host.Client {
    [Flags]
    public enum RContextType {
        TopLevel = 0x0,
        Next = 0x1,
        Break = 0x2,
        Function = 0x4,
        CCode = 0x8,
        Browser = 0x10,
        Restart = 0x20,
        Builtin = 0x40,
        Loop = Break | Next,
        Return = Function | CCode,
        Generic = Function | Browser
    }
}