// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using Microsoft.Common.Core;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.UnitTests.References {
    [ExcludeFromCodeCoverage]
    public class Dev15AssemblyLoaderAttribute : AssemblyLoaderAttribute {
        public Dev15AssemblyLoaderAttribute() 
            : base(Paths, AssembliesToResolve) {}

        private static string[] Paths { get; } = {
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetAssemblyPath()), "UnitTests.References.15.0")
        };

        private static string[] AssembliesToResolve { get; } = {
            "Microsoft.VisualStudio.CoreUtility.dll",
            "Microsoft.VisualStudio.Editor.dll",
            "Microsoft.VisualStudio.Text.Data.dll",
            "Microsoft.VisualStudio.Text.Internal.dll",
            "Microsoft.VisualStudio.Text.Logic.dll",
            "Microsoft.VisualStudio.Text.UI.dll",
            "Microsoft.VisualStudio.Text.UI.Wpf.dll"
        };
    }
}
