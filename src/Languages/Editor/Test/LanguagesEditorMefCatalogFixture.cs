// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Extensions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.StubBuilders;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Test.Fakes.Shell;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.Languages.Editor.Test {
    [AssemblyFixture]
    [ExcludeFromCodeCoverage]
    public sealed class LanguagesEditorMefCatalogFixture : LanguagesEditorMefCatalogFixtureBase { }

    [ExcludeFromCodeCoverage]
    public class LanguagesEditorMefCatalogFixtureBase : AssemblyMefCatalogFixture {
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

        protected override void AddValues(CompositionContainer container, string testName) {
            base.AddValues(container, testName);
            var editorShell = new TestEditorShell(container);
            var batch = new CompositionBatch()
                .AddValue(FileSystemStubFactory.CreateDefault())
                .AddValue<ICoreShell>(editorShell)
                .AddValue<IEditorShell>(editorShell)
                .AddValue(editorShell);
            container.Compose(batch);
        }
    }
}
