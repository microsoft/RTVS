// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.Common.Core.Test.StubBuilders;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.Text;
using Microsoft.UnitTests.Core.XUnit;
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
            return DefaultInitializeResult;
        }

        protected override void AddExports(CompositionBatch batch) {
            batch.AddValue(FileSystemStubFactory.CreateDefault());
        }

        class TestEditorSupport : IEditorSupport {
            public ICommandTarget TranslateCommandTarget(IEditorView textView, object commandTarget) => commandTarget as ICommandTarget;
            public object TranslateToHostCommandTarget(IEditorView textView, object commandTarget) => commandTarget;
            public IEditorUndoAction CreateUndoAction(IEditorView textView, IEditorBuffer textBuffer) => new TestCompoundAction();
        }

        class TestCompoundAction : IEditorUndoAction {
            public void Dispose() { }
            public void Open(string name) { }
            public void Commit() { }
        }
    }
}
