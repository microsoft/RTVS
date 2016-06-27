// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Shell;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class ExportsTest {
        [Test]
        [Category.Repl]
        public void RSessionProvider_ExportTest() {
            Lazy<IRSessionProvider> lazy = VsAppShell.Current.ExportProvider.GetExport<IRSessionProvider>();
            lazy.Should().NotBeNull();
            lazy.Value.Should().NotBeNull();
        }
    }
}
