// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.UnitTests.Core.Mef;

namespace Microsoft.UnitTests.Core.XUnit {
    [ExcludeFromCodeCoverage]
    public abstract class AssemblyMefCatalog : TestMefCatalog {
        protected override ComposablePartCatalog CreateCatalog() => CatalogFactory.CreateAssembliesCatalog(GetAssemblies().AsList());
        protected virtual IEnumerable<string> GetAssemblies() => Enumerable.Empty<string>();
    }
}