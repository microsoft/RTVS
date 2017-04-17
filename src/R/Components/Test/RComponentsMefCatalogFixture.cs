// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.R.Components.Settings;
using Microsoft.R.Components.Test.StubFactories;
using Microsoft.R.Interpreters;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Shell;
using Xunit.Sdk;

namespace Microsoft.R.Components.Test {
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public sealed class RComponentsMefCatalogFixture : AssemblyMefCatalogFixture {
        protected override IEnumerable<string> GetAssemblies() {
            return new[] {
                "Microsoft.VisualStudio.CoreUtility.dll",
                "Microsoft.VisualStudio.Text.Data.dll",
                "Microsoft.VisualStudio.Text.Logic.dll",
                "Microsoft.VisualStudio.Text.UI.dll",
                "Microsoft.VisualStudio.Text.UI.Wpf.dll",
                "Microsoft.VisualStudio.InteractiveWindow.dll",
                "Microsoft.R.Host.Client.Windows.dll",
                "Microsoft.R.Common.Core.dll",
                "Microsoft.R.Common.Core.Test.dll",
                "Microsoft.R.Components.dll",
                "Microsoft.R.Components.Test.dll",
                "Microsoft.VisualStudio.Editor.dll",
                "Microsoft.VisualStudio.Language.Intellisense.dll",
                "Microsoft.VisualStudio.Platform.VSEditor.dll",
                "System.Collections.Immutable.dll"
            };
        }

        public IExportProvider Create(ServiceManagerFixture services) => new RComponentsTestExportProvider(CreateContainer(), services);

        private class RComponentsTestExportProvider : TestExportProvider {
            private readonly ICoreShell _coreShell;

            public RComponentsTestExportProvider(CompositionContainer compositionContainer, IServiceManager services) : base(compositionContainer) {
                services.AddService<IRInstallationService>(new RInstallation());
                _coreShell = TestCoreShell.CreateFromCompositionContainer(CompositionContainer, services);
            }

            public override Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
                var batch = new CompositionBatch()
                    .AddValue<IRSettings>(RSettingsStubFactory.CreateForExistingRPath(testInput.FileSytemSafeName))
                    .AddValue(_coreShell);

                CompositionContainer.Compose(batch);
                return base.InitializeAsync(testInput, messageBus);
            }
        }
    }
}
