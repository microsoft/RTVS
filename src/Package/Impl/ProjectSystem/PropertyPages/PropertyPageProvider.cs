// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.VS.Properties;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.PropertyPages {
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    [Export(typeof(IVsProjectDesignerPageProvider))]
    internal class PropertyPageProvider : IVsProjectDesignerPageProvider {
        [ImportMany]
        private OrderPrecedenceImportCollection<IPageMetadata> PropertyPages { get; set; }

        [ImportingConstructor]
        public PropertyPageProvider(UnconfiguredProject unconfiguredProject) {
            PropertyPages = new OrderPrecedenceImportCollection<IPageMetadata>(projectCapabilityCheckProvider: unconfiguredProject);
        }

        public Task<IReadOnlyCollection<IPageMetadata>> GetPagesAsync() {
            return Task.FromResult((IReadOnlyCollection<IPageMetadata>)PropertyPages.Select(p => p.Value).ToList().AsReadOnly());
        }

        // Only here to ensure one instance per project
        [Import]
        [ExcludeFromCodeCoverage]
        internal UnconfiguredProject UnconfiguredProject { get; private set; }
    }
}
