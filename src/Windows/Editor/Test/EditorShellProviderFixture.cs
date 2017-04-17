// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.Common.Core.Test.StubBuilders;
using Microsoft.Language.Editor.Test.Settings;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Xunit.Sdk;

namespace Microsoft.Languages.Editor.Test {
    /// <summary>
    /// Provides <see cref="ICoreShell"/> with test services and MEF container
    /// </summary>
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public class EditorShellProviderFixture : CoreShellProviderFixture {
        protected override CompositionContainer CreateCompositionContainer() {
            var catalog = new EditorAssemblyMefCatalog();
            return catalog.CreateContainer();
        }

        public override async Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
            await base.InitializeAsync(testInput, messageBus);
            ServiceManager.AddService(new TestEditorSupport());

            var settings = new REditorSettings(new TestSettingsStorage());
            ServiceManager.AddService(settings);

            return DefaultInitializeResult;
        }

        protected override void AddExports(CompositionBatch batch) {
            batch.AddValue(FileSystemStubFactory.CreateDefault());
        }

        class TestEditorSupport : IApplicationEditorSupport {
            public ICommandTarget TranslateCommandTarget(ITextView textView, object commandTarget) => commandTarget as ICommandTarget;
            public object TranslateToHostCommandTarget(ITextView textView, object commandTarget) => commandTarget;
            public ICompoundUndoAction CreateCompoundAction(ITextView textView, ITextBuffer textBuffer) => new TestCompoundAction();
        }

        class TestCompoundAction : ICompoundUndoAction {
            public void Dispose() { }
            public void Open(string name) { }
            public void Commit() { }
        }
    }
}
