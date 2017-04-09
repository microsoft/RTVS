// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;

namespace Microsoft.Common.Core.Test.Fakes.Shell {
    [ExcludeFromCodeCoverage]
    public sealed class TestCompositionCatalog : ICompositionCatalog {
        public TestCompositionCatalog(CompositionContainer cc) : this(cc, cc) { }
        public TestCompositionCatalog(ICompositionService cs, ExportProvider ep) {
            CompositionService = cs;
            ExportProvider = ep;
        }

        public ICompositionService CompositionService { get; }
        public ExportProvider ExportProvider { get; }
    }
}
