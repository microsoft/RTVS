// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.Common.Core.Shell;

namespace Microsoft.VisualStudio.R.Package.Shell {
    internal sealed class CompositionCatalog : ICompositionCatalog {
        public CompositionCatalog(ICompositionService cs, ExportProvider ep) {
            CompositionService = cs;
            ExportProvider = ep;
        }

        public ICompositionService CompositionService { get; }
        public ExportProvider ExportProvider { get; }
    }
}
