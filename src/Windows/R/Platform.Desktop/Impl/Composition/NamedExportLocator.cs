// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Common.Core;

namespace Microsoft.R.Platform.Composition {
    public sealed class NamedExportLocator<TExport> {
        [ImportMany]
        private IEnumerable<Lazy<TExport, INamedExport>> Exports { get; set; }

        public NamedExportLocator(ICompositionService cs) {
            cs.SatisfyImportsOnce(this);
        }

        public TExport GetExport(string name) 
            => Exports.FirstOrDefault(e => e.Metadata.Name.EqualsOrdinal(name)).Value;
    }
}
