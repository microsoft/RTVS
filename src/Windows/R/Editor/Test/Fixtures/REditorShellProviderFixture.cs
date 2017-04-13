﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Language.Editor.Test.Settings;
using Microsoft.Languages.Editor.Test;
using Microsoft.R.Editor.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Xunit.Sdk;

namespace Microsoft.R.Editor.Test {
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public class REditorShellProviderFixture : EditorShellProviderFixture {
        protected override CompositionContainer CreateCompositionContainer() {
            var catalog = new REditorAssemblyMefCatalog();
            return catalog.CreateContainer();
        }

        public override async Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            await base.InitializeAsync(testInput, messageBus);
            var settings = new REditorSettings(new TestSettingsStorage());
            ServiceManager.AddService(settings);
            return DefaultInitializeResult;
        }
    }
}
