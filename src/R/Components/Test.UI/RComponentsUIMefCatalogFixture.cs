using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Test.UI.Fakes;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Components.Test.UI {
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public sealed class RComponentsUIMefCatalogFixture : AssemblyMefCatalogFixture {
        protected override IEnumerable<string> GetBinDirectoryAssemblies() {
            return new[] {
                "Microsoft.VisualStudio.InteractiveWindow.dll",
                "Microsoft.R.Host.Client.dll",
                "Microsoft.R.Common.Core.dll",
                "Microsoft.R.Common.Core.Test.dll",
                "Microsoft.R.Components.dll",
                // Do not add Microsoft.R.Components.Test.dll here cause it will result in cardinality mismatch
                "Microsoft.R.Components.Test.UI.dll",
            };
        }

        protected override IEnumerable<string> GetNugetAssemblies() {
            return new[] {
                "Microsoft.VisualStudio.CoreUtility.dll",
                "Microsoft.VisualStudio.Text.Data.dll",
                "Microsoft.VisualStudio.Text.Logic.dll",
                "Microsoft.VisualStudio.Text.UI.dll",
                "Microsoft.VisualStudio.Text.UI.Wpf.dll"
            };
        }

        protected override IEnumerable<string> GetVsAssemblies() {
            return new[] {
                "Microsoft.VisualStudio.Editor.dll",
                "Microsoft.VisualStudio.Language.Intellisense.dll",
                "Microsoft.VisualStudio.Platform.VSEditor.dll"
            };
        }

        protected override void AddValues(CompositionContainer container) {
            base.AddValues(container);
            var coreShell = new TestCoreShell(container);
            var batch = new CompositionBatch()
                .AddValue<ICoreShell>(coreShell)
                .AddValue(coreShell);
            container.Compose(batch);
        }
    }
}
