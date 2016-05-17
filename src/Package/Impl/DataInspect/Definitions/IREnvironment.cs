// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal interface IREnvironment {
        string Name { get; }

        string EnvironmentExpression { get; }

        REnvironmentKind Kind { get; }
    }
}
