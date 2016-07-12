// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.Plots.Implementation.View;
using Microsoft.R.Components.Sql;
using Microsoft.UnitTests.Core.XUnit;

namespace Microsoft.R.Components.Test.Plots {
    [ExcludeFromCodeCoverage]
    [Category.Sql]
    public class SqlDbTest  {
        [Test]
        public void StringConverter() {
            string s = null;
            s.SqlClientToOdbc().Should().BeNull();
            s.OdbcToSqlClient().Should().BeNull();

            var odbc = "Driver={SQL Server};Server=MIKHAILA1;Database=AdventureWorks2016CTP3;Trusted_Connection=yes";
            var sql = odbc.OdbcToSqlClient();
            sql.Should().Be("Data Source=MIKHAILA1;Initial Catalog=AdventureWorks2016CTP3;Integrated Security=True");
            sql.SqlClientToOdbc().Should().Be(odbc);

            odbc = "Driver={SQL Server};Server=MIKHAILA1;Database=AdventureWorks2016CTP3;Uid=MeMyselfAndI;Pwd=Password";
            sql = odbc.OdbcToSqlClient();
            sql.Should().Be("Data Source=MIKHAILA1;Initial Catalog=AdventureWorks2016CTP3;Integrated Security=False;User ID=MeMyselfAndI;Password=Password");
            sql.SqlClientToOdbc().Should().Be(odbc);
        }
    }
}
