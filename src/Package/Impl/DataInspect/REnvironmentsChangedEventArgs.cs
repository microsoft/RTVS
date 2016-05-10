// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class REnvironmentsChangedEventArgs : EventArgs {
        public REnvironmentsChangedEventArgs(IReadOnlyList<REnvironment> searchPath, IReadOnlyList<REnvironment> traceback) {
            SearchPath = searchPath.Where(x => !x.Name.EqualsOrdinal(".rtvs")).ToArray();
            Traceback = traceback;
        }

        public IReadOnlyList<REnvironment> SearchPath { get; }

        public IReadOnlyList<REnvironment> Traceback { get; }
    }
}
