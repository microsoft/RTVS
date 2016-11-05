// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    public sealed class IntRangeDiscoverer : IDataDiscoverer {
        /// <inheritdoc/>
        public IEnumerable<object[]> GetData(IAttributeInfo dataAttribute, IMethodInfo testMethod) {
            var args = dataAttribute.GetConstructorArguments().ToList();
            var start = 0;
            var count = 1;
            var step = 1;
            if (args.Count == 1) {
                count = (int) args[0];
            } else if (args.Count >= 2) {
                start = (int)args[0];
                count = (int)args[1];
            }

            if (args.Count >= 3) {
                step = (int)args[2];
            }

            for (var i = 0; i < count; i++) {
                yield return new object[] { start + i * step };
            }
        }

        /// <inheritdoc/>
        public bool SupportsDiscoveryEnumeration(IAttributeInfo dataAttribute, IMethodInfo testMethod) {
            return true;
        }
    }
}