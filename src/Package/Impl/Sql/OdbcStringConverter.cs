// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Data.Odbc;
using System.Data.SqlClient;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal static class OdbcStringConverter {
        private const string _odbcSqlDriver = "{SQL Server}";
        private const string _odbcDriverKey = "Driver";
        private const string _odbcServerKey = "Server";
        private const string _odbcDatabaseKey = "Database";
        private const string _odbcUidKey = "Uid";
        private const string _odbcPasswordKey = "Pwd";
        private const string _odbcTrustedConnectionKey = "Trusted_Connection";

        /// <summary>
        /// Converts SQL Client (.NET) connection string to the ODBC connection string.
        /// </summary>
        public static string SqlClientToOdbc(this string sqlClientString) {
            try {
                var sql = new SqlConnectionStringBuilder(sqlClientString);
                var odbc = new OdbcConnectionStringBuilder();
                odbc[_odbcDriverKey] = _odbcSqlDriver;
                odbc[_odbcServerKey] = sql.DataSource;
                odbc[_odbcDatabaseKey] = sql.InitialCatalog;
                if (sql.IntegratedSecurity) {
                    odbc[_odbcTrustedConnectionKey]  = "yes";
                } else {
                    odbc[_odbcUidKey] = sql.UserID;
                    odbc[_odbcPasswordKey] = sql.Password;
                }
                return odbc.ConnectionString;
            } catch (ArgumentException) { }
            return string.Empty;
        }

        /// <summary>
        /// Converts ODBC connection string to the SQL Client (.NET).
        /// </summary>
        public static string OdbcToSqlClient(this string odbcString) {
            try {
                var odbc = new OdbcConnectionStringBuilder(odbcString);
                var server= odbc.GetValue(_odbcServerKey);
                var database = odbc.GetValue(_odbcDatabaseKey);
                if (!string.IsNullOrWhiteSpace(server) && !string.IsNullOrWhiteSpace(database)) {
                    var sql = new SqlConnectionStringBuilder();
                    sql.DataSource = server;
                    sql.InitialCatalog = database;

                    if (odbc.ContainsKey(_odbcUidKey)) {
                        //Standard Connection
                        sql.IntegratedSecurity = false;
                        sql.UserID = odbc.GetValue(_odbcUidKey);
                        sql.Password = odbc.GetValue(_odbcPasswordKey);
                    } else {
                        //Trusted Connection
                        sql.IntegratedSecurity = true;
                    }
                    return sql.ConnectionString;
                }
            } catch(ArgumentException) { }

            return string.Empty;
        }

        private static string GetValue(this OdbcConnectionStringBuilder odbc, string key) {
            object oValue;
            odbc.TryGetValue(key, out oValue);
            return oValue as string;
        }
    }
}
