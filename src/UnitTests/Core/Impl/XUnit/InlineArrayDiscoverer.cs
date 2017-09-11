// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    public class InlineArrayDiscoverer : IDataDiscoverer {
        /// <inheritdoc/>
        public virtual IEnumerable<object[]> GetData(IAttributeInfo dataAttribute, IMethodInfo testMethod) {
            var args = (IEnumerable<object>)dataAttribute.GetConstructorArguments().Single() ?? Enumerable.Empty<object>();
            yield return new object[] { args.ToArray() };
        }

        /// <inheritdoc/>
        public virtual bool SupportsDiscoveryEnumeration(IAttributeInfo dataAttribute, IMethodInfo testMethod) {
            return true;
        }
    }
}