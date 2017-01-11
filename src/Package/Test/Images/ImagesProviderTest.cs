// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Editor.Imaging;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Shell;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Images {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class ImagesProviderTest {
        [Test]
        [Category.Project.Services]
        public void ImagesProvider_Test() {
            IImagesProvider p = VsAppShell.Current.ExportProvider.GetExportedValue<IImagesProvider>();
            p.Should().NotBeNull();

            p.GetFileIcon("foo.R").Should().NotBeNull();
            p.GetFileIcon("foo.rproj").Should().NotBeNull();
            p.GetFileIcon("foo.rdata").Should().NotBeNull();
            p.GetFileIcon("foo.rd").Should().NotBeNull();
            p.GetFileIcon("foo.rmd").Should().NotBeNull();
            p.GetFileIcon("foo.sql").Should().NotBeNull();

            p.GetImage("RProjectNode").Should().NotBeNull();
            p.GetImage("RFileNode").Should().NotBeNull();
            p.GetImage("RDataFileNode").Should().NotBeNull();
            p.GetImage("RdFileNode").Should().NotBeNull();
            p.GetImage("RMdFileNode").Should().NotBeNull();
            p.GetImage("SQLFileNode").Should().NotBeNull();
            p.GetImage("ProcedureFileNode").Should().NotBeNull();
        }
    }
}
