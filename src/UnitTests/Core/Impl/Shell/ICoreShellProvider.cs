// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.UnitTests.Core.Shell {
    public interface ICoreShellProvider: IMethodFixture {
        ICoreShell CoreShell { get; }
    }
}
