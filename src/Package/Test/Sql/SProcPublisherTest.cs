// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.R.Components.Sql.Publish;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class SProcPublisherTest {
        private readonly PackageTestFilesFixture _files;
        private readonly TestCoreShell _shell;
        private readonly IProjectSystemServices _pss;
        private readonly IDacPackageServices _dacServices;

        public SProcPublisherTest(PackageTestFilesFixture files) {
            _files = files;
            _shell = TestCoreShell.CreateSubstitute();

            _shell.ServiceManager.RemoveService<IFileSystem>();
            _shell.ServiceManager.AddService(new WindowsFileSystem());

            _pss = Substitute.For<IProjectSystemServices>();
            _dacServices = Substitute.For<IDacPackageServices>();
        }


        [CompositeTest(ThreadType.UI)]
        [InlineData("sqlcode1.r")]
        public void PublishDacpac(string rFile) {
            var settings = new SqlSProcPublishSettings { TargetType = PublishTargetType.Dacpac };

            SetupProjectMocks("project.rproj");

            var builder = Substitute.For<IDacPacBuilder>();
            builder.When(x => x.Build(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>())).Do(c => {
                c.Args()[0].Should().Be("project.dacpac");
                c.Args()[1].Should().Be("project");

                var e = c.Args()[2] as IEnumerable<string>;
                e.Should().HaveCount(1);
                e.First().Should().StartWith("CREATE PROCEDURE ProcName");
            });

            _dacServices.GetBuilder().Returns(builder);

            var files = new [] { Path.Combine(_files.DestinationPath, rFile) };
            var publisher = new SProcPublisher(_shell.Services, _pss, _dacServices);
            publisher.Publish(settings, files);

            builder.Received(1).Build(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>());
        }

        private void SetupProjectMocks(string fileName) {
            var dteProj = Substitute.For<EnvDTE.Project>();
            dteProj.FullName.Returns(fileName);
            dteProj.Name.Returns(Path.GetFileNameWithoutExtension(fileName));

            object ext;
            var hier = Substitute.For<IVsHierarchy>();
            hier.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out ext).Returns((c) => {
                c[2] = dteProj;
                return VSConstants.S_OK;
            });
            _pss.GetSelectedProject<IVsHierarchy>().Returns(hier);
        }
    }
}
