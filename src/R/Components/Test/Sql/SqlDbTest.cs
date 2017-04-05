// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using FluentAssertions;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Sql;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.Win32;
using NSubstitute;
using Xunit;

namespace Microsoft.R.Components.Test.Sql {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class ConnectiionStringTest {
        [CompositeTest]
        [InlineData(null, null)]
        [InlineData("Driver={SQL Server};Server=ABC;Database=AdventureWorks2016CTP3;Trusted_Connection=yes",
                    "Data Source=ABC;Initial Catalog=AdventureWorks2016CTP3;Integrated Security=True;TrustServerCertificate=False")]
        [InlineData("Driver={SQL Server};Server=ABC;Database=AdventureWorks2016CTP3;Uid=MeMyselfAndI;Pwd=Password",
                    "Data Source=ABC;Initial Catalog=AdventureWorks2016CTP3;Integrated Security=False;User ID=MeMyselfAndI;Password=Password;TrustServerCertificate=False")]
        [InlineData("Driver={ODBC Driver 13 for SQL Server};Server=ABC;Database=AdventureWorks2016CTP3;Authentication=ActiveDirectoryIntegrated;Trusted_Connection=yes",
                    "Data Source=ABC;Initial Catalog=AdventureWorks2016CTP3;Integrated Security=True;TrustServerCertificate=False;Authentication=\"Active Directory Integrated\"")]
        public void Odbc2Sql(string odbc, string sql) {
            odbc.OdbcToSqlClient().Should().Be(sql);
            sql.SqlClientToOdbc().Should().Be(odbc);
        }

        [CompositeTest]
        [InlineData("1.0", 1033, false)]
        [InlineData("1.0", 1049, false)]
        [InlineData("14.0", 1049, false)]
        [InlineData("2015.131.4413.45", 2052, false)]
        [InlineData("2015.131.4413.46", 1033, true)]
        [InlineData("2015.131.4413.47", 2052, true)]
        [InlineData("2015.132.4413.45", 2052, true)]
        [InlineData("2016.131.4413.45", 2052, true)]
        public void OdbcDriverCheck(string version, uint lcid, bool expected) {
            var coreShell = Substitute.For<ICoreShell>();
            var driverPath = @"c:\windows\system32\driver.dll";

            var fs = Substitute.For<IFileSystem>();
            fs.FileExists(Arg.Any<string>()).Returns(true);
            fs.GetFileVersion(driverPath).Returns(new Version(version));
            coreShell.Services.FileSystem.Returns(fs);

            var registry = Substitute.For<IRegistry>();
            coreShell.Services.Registry.Returns(registry);

            var odbc13Key = Substitute.For<IRegistryKey>();
            odbc13Key.GetValue("Driver").Returns(driverPath);

            var hlkm = Substitute.For<IRegistryKey>();
            hlkm.OpenSubKey(@"SOFTWARE\ODBC\ODBCINST.INI\ODBC Driver 13 for SQL Server").Returns(odbc13Key);

            registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).Returns(hlkm);
            coreShell.AppConstants.LocaleId.Returns(lcid);

            coreShell.When(x => x.ShowErrorMessage(Arg.Any<string>())).Do(x => {
                var arg = x.Args()[0] as string;
                arg.Should().Contain(CultureInfo.GetCultureInfo((int)lcid).Name);
            });

            coreShell.Services.ProcessServices.When(x => x.Start(Arg.Any<string>())).Do(x => {
                var arg = x.Args()[0] as string;
                arg.Should().Contain(CultureInfo.GetCultureInfo((int)lcid).Name);
            });

            var service = new DbConnectionService(coreShell);
            service.CheckSqlOdbcDriverVersion().Should().Be(expected);

            if(!expected) {
                coreShell.Received(1).ShowErrorMessage(Arg.Any<string>());
                coreShell.Services.ProcessServices.Received(1).Start(Arg.Any<string>());
            }
        }
    }
}
