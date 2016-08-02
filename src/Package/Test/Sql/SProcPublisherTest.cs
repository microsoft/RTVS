// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.R.Components.Sql;
using Microsoft.SqlServer.Dac;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Sql.Publish;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class SProcPublisherTest {
        private const string sqlProjectName = "db.sqlproj";

        private readonly PackageTestFilesFixture _files;
        private readonly IApplicationShell _appShell;
        private readonly IProjectSystemServices _pss;
        private readonly IDacPackageServices _dacServices;

        public SProcPublisherTest(PackageTestFilesFixture files) {
            _files = files;
            _appShell = Substitute.For<IApplicationShell>();
            _pss = Substitute.For<IProjectSystemServices>();
            _dacServices = Substitute.For<IDacPackageServices>();
        }


        [CompositeTest(ThreadType.UI)]
        [InlineData("sqlcode1.r")]
        public void PublishDacpac(string rFile) {
            var fs = new FileSystem();
            var settings = new SqlSProcPublishSettings();
            settings.TargetType = PublishTargetType.Dacpac;

            SetupProjectMocks("project.rproj");

            var builder = Substitute.For<IDacPacBuilder>();
            builder.When(x => x.Build(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>())).Do(c => {
                c.Args()[0].Should().Be("project.dacpac");
                c.Args()[1].Should().Be("project");

                var e = c.Args()[2] as IEnumerable<string>;
                e.Should().HaveCount(1);
                e.First().Should().StartWith("CREATE PROCEDURE ProcName");
            });

            _dacServices.GetBuilder(null).ReturnsForAnyArgs(builder);

            var files = new string[] { Path.Combine(_files.DestinationPath, rFile) };
            var publisher = new SProcPublisher(_appShell, _pss, fs, _dacServices);
            publisher.Publish(settings, files);

            builder.Received(1).Build(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>());
        }

        [CompositeTest(ThreadType.UI)]
        [InlineData("sqlcode1.r")]
        public void PublishDatabase(string rFile) {
            var fs = new FileSystem();
            var settings = new SqlSProcPublishSettings();
            settings.TargetType = PublishTargetType.Database;

            var odbc = new OdbcConnectionStringBuilder();
            odbc[ConnectionStringConverter.OdbcDriverKey] = "SQL Server";
            odbc[ConnectionStringConverter.OdbcServerKey] = "(local)";
            odbc[ConnectionStringConverter.OdbcDatabaseKey] = "AventureWorks";
            settings.TargetDatabaseConnection = odbc.ConnectionString;

            SetupProjectMocks("project.rproj");

            var builder = Substitute.For<IDacPacBuilder>();
            _dacServices.GetBuilder(null).ReturnsForAnyArgs(builder);
            _dacServices.When(x => x.Deploy(Arg.Any<DacPackage>(), Arg.Any<string>(), Arg.Any<string>())).Do(c => {
                ((string)c.Args()[1]).Should().Be("Data Source=(local);Initial Catalog=AventureWorks;Integrated Security=True");
            });

            var files = new string[] { Path.Combine(_files.DestinationPath, rFile) };
            var publisher = new SProcPublisher(_appShell, _pss, fs, _dacServices);
            publisher.Publish(settings, files);

            _dacServices.Received(1).Deploy(Arg.Any<DacPackage>(), Arg.Any<string>(), Arg.Any<string>());
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
