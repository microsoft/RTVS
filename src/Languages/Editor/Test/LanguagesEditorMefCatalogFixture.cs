// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.Test.StubBuilders;
using Microsoft.Language.Editor.Test.Settings;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Components.Controller;
using Microsoft.R.Editor.Settings;
using Microsoft.UnitTests.Core.Mef;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Xunit.Sdk;

namespace Microsoft.Languages.Editor.Test {
    // Fixture doesn't import itself. Use AssemblyFixtureImportAttribute
    [ExcludeFromCodeCoverage]
    public class LanguagesEditorMefCatalogFixture : AssemblyMefCatalogFixture {
        protected override IEnumerable<string> GetAssemblies() {
            return new[] {
                "Microsoft.VisualStudio.CoreUtility.dll",
                "Microsoft.VisualStudio.Text.Data.dll",
                "Microsoft.VisualStudio.Text.Internal.dll",
                "Microsoft.VisualStudio.Text.Logic.dll",
                "Microsoft.VisualStudio.Text.UI.dll",
                "Microsoft.VisualStudio.Text.UI.Wpf.dll",
                "Microsoft.Languages.Editor.dll",
                "Microsoft.R.Host.Client.dll",
                "Microsoft.R.Common.Core.dll",
                "Microsoft.R.Common.Core.Test.dll",
                "Microsoft.R.Components.dll",
                "Microsoft.R.Components.Test.dll",
                "Microsoft.VisualStudio.Editor.dll",
                "Microsoft.VisualStudio.Language.Intellisense.dll",
                "Microsoft.VisualStudio.Platform.VSEditor.dll"
            };
        }

        public virtual IExportProvider Create() => new LanguagesEditorTestExportProvider(CreateContainer());

        protected class LanguagesEditorTestExportProvider : TestExportProvider {
            private readonly ICoreShell _coreShell;
            public LanguagesEditorTestExportProvider(CompositionContainer compositionContainer) : base(compositionContainer) {
                var tcs = new TestCoreShell(new TestCompositionCatalog(compositionContainer));
                tcs.ServiceManager.AddService(new TestEditorSupport());
                _coreShell = tcs;

                // TODO: HACK - remove after REditorSettings turn into service.
                REditorSettings.Initialize(new TestSettingsStorage());
            }

            public override Task<Task<RunSummary>> InitializeAsync(ITestInput testInput, IMessageBus messageBus) {
                var batch = new CompositionBatch()
                    .AddValue(FileSystemStubFactory.CreateDefault())
                    .AddValue(_coreShell);
                CompositionContainer.Compose(batch);
                return base.InitializeAsync(testInput, messageBus);
            }
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
