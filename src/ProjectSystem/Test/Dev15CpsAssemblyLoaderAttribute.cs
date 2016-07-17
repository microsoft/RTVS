// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Test {
    [ExcludeFromCodeCoverage]
    public class Dev15CpsAssemblyLoaderAttribute : AssemblyLoaderAttribute {
        public Dev15CpsAssemblyLoaderAttribute() 
            : base(Paths, AssembliesToResolve) {}

        private static string[] Paths { get; } = {
            UnitTests.Core.Paths.VsPrivateAssemblies,
            UnitTests.Core.Paths.VsCommonExtensions
        };

        private static string[] AssembliesToResolve { get; } = {
            "Microsoft.VisualStudio.ProjectSystem.dll",
            "Microsoft.VisualStudio.ProjectSystem.VS.dll"
        };
    }
}
