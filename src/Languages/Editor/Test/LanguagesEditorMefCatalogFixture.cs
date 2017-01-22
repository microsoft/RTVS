// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.Common.Core.Test.StubBuilders;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Fakes.Shell;
using Microsoft.R.Components.Settings;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit.Sdk;

namespace Microsoft.Languages.Editor.Test {
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    [ExcludeFromCodeCoverage]
    public class LanguagesEditorMefCatalogFixture : AssemblyMefCatalogFixture {
        protected override IEnumerable<string> GetBinDirectoryAssemblies() {
            return new[] {
                "Microsoft.Languages.Editor.dll",
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
                "Microsoft.VisualStudio.Text.Internal.dll",
                "Microsoft.VisualStudio.Text.Logic.dll",
                "Microsoft.VisualStudio.Text.UI.dll",
                "Microsoft.VisualStudio.Text.UI.Wpf.dll"
            };
        }

        public virtual IExportProvider Create(CoreServicesFixture coreServices) => new LanguagesEditorTestExportProvider(CreateContainer(), coreServices);

        protected class LanguagesEditorTestExportProvider : TestExportProvider {
            private readonly CoreServicesFixture _coreServices;
            public LanguagesEditorTestExportProvider(CompositionContainer compositionContainer, CoreServicesFixture coreServices) : base(compositionContainer) {
                _coreServices = coreServices;
            }

            public override Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
                var editorShell = new TestEditorShell(CompositionContainer, _coreServices);
                var batch = new CompositionBatch()
                    .AddValue(FileSystemStubFactory.CreateDefault())
                    .AddValue<ICoreShell>(editorShell)
                    .AddValue<IEditorShell>(editorShell)
                    .AddValue(editorShell);
                CompositionContainer.Compose(batch);
                return base.InitializeAsync(testInput, messageBus);
            }
        }
    }
}
