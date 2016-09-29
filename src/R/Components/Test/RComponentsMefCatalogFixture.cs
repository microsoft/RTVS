// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Shell;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Test.StubFactories;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Components.Test {
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public sealed class RComponentsMefCatalogFixture : AssemblyMefCatalogFixture {
        protected override IEnumerable<string> GetBinDirectoryAssemblies() {
            return new[] {
                "Microsoft.VisualStudio.InteractiveWindow.dll",
                "Microsoft.R.Host.Client.dll",
                "Microsoft.R.Common.Core.dll",
                "Microsoft.R.Common.Core.Test.dll",
                "Microsoft.R.Components.dll",
                "Microsoft.R.Components.Test.dll",
            };
        }

        protected override IEnumerable<string> GetVsAssemblies() {
            return new[] {
                "Microsoft.VisualStudio.Editor.dll",
                "Microsoft.VisualStudio.Language.Intellisense.dll",
                "Microsoft.VisualStudio.Platform.VSEditor.dll"
            };
        }

        protected override IEnumerable<string> GetLoadedAssemblies() {
            return new[] {
                "Microsoft.VisualStudio.CoreUtility.dll",
                "Microsoft.VisualStudio.Text.Data.dll",
                "Microsoft.VisualStudio.Text.Logic.dll",
                "Microsoft.VisualStudio.Text.UI.dll",
                "Microsoft.VisualStudio.Text.UI.Wpf.dll"
            };
        }

        protected override void AddValues(CompositionContainer container) {
            base.AddValues(container);
            var coreShell = new TestCoreShell(container);
            var batch = new CompositionBatch()
                .AddValue<IRSettings>(RSettingsStubFactory.CreateForExistingRPath())
                .AddValue<ICoreShell>(coreShell)
                .AddValue(coreShell);
            container.Compose(batch);
        }
    }
}
