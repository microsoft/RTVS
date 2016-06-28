// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.StubBuilders;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Support.Test {
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public sealed class RSupportMefCatalogFixture : RSupportMefCatalogFixtureBase { }

    [ExcludeFromCodeCoverage]
    public class RSupportMefCatalogFixtureBase : LanguagesEditorMefCatalogFixtureBase {
        protected override IEnumerable<string> GetBinDirectoryAssemblies() => base.GetBinDirectoryAssemblies().Concat(new[] {
            "Microsoft.R.Support",
            "Microsoft.R.Support.Test"
        });

        protected override void AddValues(CompositionContainer container) {
            base.AddValues(container);
            var batch = new CompositionBatch()
                .AddValue<IRSettings>(new TestRToolsSettings());
            container.Compose(batch);
        }
    }
}
