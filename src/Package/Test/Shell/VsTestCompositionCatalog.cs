// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Test.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    [ExcludeFromCodeCoverage]
    public sealed class VsTestCompositionCatalog {
        private static readonly string[] _assemblies = {
            "Microsoft.VisualStudio.Shell.Mocks.dll",
            "Microsoft.VisualStudio.R.Package.dll",
            "Microsoft.VisualStudio.R.Package.Test.dll",
            "Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.dll",
            "System.Collections.Immutable.dll"
        };

        public static EditorTestCompositionCatalog Current { get; } = new EditorTestCompositionCatalog(_assemblies);
    }
}
