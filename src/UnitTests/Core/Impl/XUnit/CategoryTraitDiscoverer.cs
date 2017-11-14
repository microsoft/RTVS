// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.UnitTests.Core.XUnit {
    public class CategoryTraitDiscoverer : ITraitDiscoverer {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute) {
            var categories = traitAttribute.GetNamedArgument<string[]>(nameof(CategoryAttribute.Categories));
            return categories.Select(category => new KeyValuePair<string, string>("Category", category));
        }
    }
}