// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.R.Debugger;

namespace Microsoft.VisualStudio.R.Package.DataInspect.Viewers {
    [Export(typeof(IObjectDetailsViewerAggregator))]
    public sealed class ObjectDetailsViewerAggregator : IObjectDetailsViewerAggregator {
        [ImportMany]
        private IEnumerable<Lazy<IObjectDetailsViewer>> Viewers { get; set; }

        public IObjectDetailsViewer GetViewer(DebugValueEvaluationResult result) {
            Lazy<IObjectDetailsViewer> lazyViewer = Viewers.FirstOrDefault(x => x.Value.CanView(result));
            return lazyViewer?.Value;
        }
    }
}
