// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    public class TestFrameworkDiscoverer : XunitTestFrameworkDiscoverer {
        public TestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink, IList<AssemblyLoaderAttribute> assemblyLoaders, IXunitTestCollectionFactory collectionFactory = null)
            : base(assemblyInfo, sourceProvider, diagnosticMessageSink, collectionFactory) {

            foreach (var assemblyLoader in assemblyLoaders) {
                DisposalTracker.Add(assemblyLoader);
            }
        }
    }
}