// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.Languages.Editor.Test;
using Microsoft.R.Components.Settings;
using Microsoft.R.Support.Test.Utility;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Xunit.Sdk;

namespace Microsoft.R.Support.Test {
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    [ExcludeFromCodeCoverage]
    public class RSupportMefCatalogFixture : LanguagesEditorMefCatalogFixture {
        protected override IEnumerable<string> GetAssemblies() => base.GetAssemblies().Concat(new[] {
            "Microsoft.VisualStudio.InteractiveWindow.dll",
            "Microsoft.R.Support",
            "Microsoft.R.Support.Test"
        });

        public override IExportProvider Create(CoreServicesFixture coreServices)
            => new RSupportTestExportProvider(CreateContainer(), coreServices);

        protected class RSupportTestExportProvider : LanguagesEditorTestExportProvider {
            public RSupportTestExportProvider(CompositionContainer compositionContainer, CoreServicesFixture coreServices) 
                : base(compositionContainer, coreServices) {}

            public override async Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
                var result = await base.InitializeAsync(testInput, messageBus);
                var batch = new CompositionBatch()
                    .AddValue<IRSettings>(new TestRToolsSettings(testInput.FileSytemSafeName));
                CompositionContainer.Compose(batch);
                return result;
            }
        }
    }
}
