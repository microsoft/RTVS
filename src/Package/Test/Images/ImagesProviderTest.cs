// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Imaging;
using Microsoft.Common.Core.Services;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Images {
    [ExcludeFromCodeCoverage]
    [Category.Project.Services]
    [Collection(CollectionNames.NonParallel)]
    public class ImageServiceTest {
        private readonly IServiceContainer _services;

        public ImageServiceTest(IServiceContainer services) {
            _services = services;
        }

        [Test]
        public void Test01() {
            var service = _services.GetService<IImageService>();
            service.Should().NotBeNull();

            service.GetFileIcon("foo.R").Should().NotBeNull();
            service.GetFileIcon("foo.rproj").Should().NotBeNull();
            service.GetFileIcon("foo.rdata").Should().NotBeNull();
            service.GetFileIcon("foo.rd").Should().NotBeNull();
            service.GetFileIcon("foo.rmd").Should().NotBeNull();
            service.GetFileIcon("foo.sql").Should().NotBeNull();

            service.GetImage("RProjectNode").Should().NotBeNull();
            service.GetImage("RFileNode").Should().NotBeNull();
            service.GetImage("RDataFileNode").Should().NotBeNull();
            service.GetImage("RdFileNode").Should().NotBeNull();
            service.GetImage("RMdFileNode").Should().NotBeNull();
            service.GetImage("SQLFileNode").Should().NotBeNull();
            service.GetImage("ProcedureFileNode").Should().NotBeNull();
        }
    }
}
