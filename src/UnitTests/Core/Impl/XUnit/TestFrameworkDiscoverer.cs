// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    internal class TestFrameworkDiscoverer : XunitTestFrameworkDiscoverer {
        public TestFrameworkDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider, IMessageSink diagnosticMessageSink, IList<AssemblyLoaderAttribute> assemblyLoaders, IXunitTestCollectionFactory collectionFactory = null)
            : base(assemblyInfo, sourceProvider, diagnosticMessageSink, collectionFactory) {
            foreach (var assemblyLoader in assemblyLoaders) {
                DisposalTracker.Add(assemblyLoader);
            }
        }
    }
}