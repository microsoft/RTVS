// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test {
    [ExcludeFromCodeCoverage]
    public class CpsAssemblyLoaderAttribute : VsAssemblyLoaderAttribute {
        public CpsAssemblyLoaderAttribute() 
            : base(AssembliesToResolve) {}

        private static string[] AssembliesToResolve { get; } = {
            "Microsoft.VisualStudio.ProjectSystem.dll",
            "Microsoft.VisualStudio.ProjectSystem.VS.dll"
        };
    }
}
