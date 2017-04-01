// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Data.Odbc;
using System.Data.SqlClient;

namespace Microsoft.R.Components.Sql {
    public static class ConnectionStringConverter {
        public const string OdbcSqlDriver = "{SQL Server}";

        public const string OdbcDriverKey = "Driver";
        public const string OdbcServerKey = "Server";
        public const string OdbcDatabaseKey = "Database";
        public const string OdbcUidKey = "Uid";
        public const string OdbcPasswordKey = "Pwd";
        public const string OdbcTrustedConnectionKey = "Trusted_Connection";
        public const string OdbcAuthenticationKey = "Authentication";
        public const string OdbcTrustServerCertificateKey = "TrustServerCertificate";

        /// <summary>
        /// Converts SQL Client (.NET) connection string to the ODBC connection string.
        /// </summary>
        public static string SqlClientToOdbc(this string sqlClientString) {
            if (string.IsNullOrEmpty(sqlClientString)) {
                return null;
            }
            try {
                var sql = new SqlConnectionStringBuilder(sqlClientString);
                var odbc = new OdbcConnectionStringBuilder {
                    [OdbcDriverKey] = OdbcSqlDriver,
                    [OdbcServerKey] = sql.DataSource,
                    [OdbcDatabaseKey] = sql.InitialCatalog
                };

                if (sql.Authentication != SqlAuthenticationMethod.NotSpecified) {
                    odbc[OdbcAuthenticationKey] = Enum.GetName(typeof(SqlAuthenticationMethod), sql.Authentication);
                }

                if (sql.IntegratedSecurity) {
                    odbc[OdbcTrustedConnectionKey] = "yes";
                }
                if (sql.TrustServerCertificate) {
                    odbc[OdbcTrustServerCertificateKey] = "yes";
                }

                odbc[OdbcUidKey] = sql.UserID;
                odbc[OdbcPasswordKey] = sql.Password;

                return odbc.ConnectionString;
            } catch (ArgumentException) { }
            return null;
        }

        /// <summary>
        /// Converts ODBC connection string to the SQL Client (.NET).
        /// </summary>
        public static string OdbcToSqlClient(this string odbcString) {
            if (string.IsNullOrEmpty(odbcString)) {
                return null;
            }
            try {
                var odbc = new OdbcConnectionStringBuilder(odbcString);
                var server = odbc.GetValue(OdbcServerKey);
                var database = odbc.GetValue(OdbcDatabaseKey);
                if (!string.IsNullOrWhiteSpace(server) && !string.IsNullOrWhiteSpace(database)) {
                    var sql = new SqlConnectionStringBuilder {
                        DataSource = server,
                        InitialCatalog = database,
                        UserID = odbc.GetValue(OdbcUidKey),
                        Password = odbc.GetValue(OdbcPasswordKey)
                    };

                    // If no password and user name, assume integrated authentication
                    sql.IntegratedSecurity = string.IsNullOrEmpty(sql.UserID) && string.IsNullOrEmpty(sql.Password);
                    sql.TrustServerCertificate = string.Compare(odbc.GetValue(OdbcTrustServerCertificateKey), "yes", StringComparison.OrdinalIgnoreCase) == 0;

                    // Translate authentication method
                    if (odbc.ContainsKey(OdbcAuthenticationKey)) {
                        SqlAuthenticationMethod authMethod;
                        if (Enum.TryParse(odbc.GetValue(OdbcAuthenticationKey), out authMethod)) {
                            sql.Authentication = authMethod;
                        }
                    }

                    return sql.ConnectionString;
                }
            } catch (ArgumentException) { }
            return null;
        }

        public static string GetValue(this string odbcString, string key) {
            var odbc = new OdbcConnectionStringBuilder(odbcString);
            return odbc.GetValue(key);
        }

        private static string GetValue(this OdbcConnectionStringBuilder odbc, string key) {
            object oValue;
            odbc.TryGetValue(key, out oValue);
            return oValue as string;
        }
    }
}
