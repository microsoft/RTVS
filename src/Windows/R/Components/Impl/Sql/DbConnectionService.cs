// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI;
using Microsoft.Data.ConnectionUI;
using Microsoft.Win32;

namespace Microsoft.R.Components.Sql {
    [Export(typeof(IDbConnectionService))]
    internal sealed class DbConnectionService : IDbConnectionService {
        private const string DefaultSqlConnectionString = "Data Source=(local);Integrated Security=true";

        private readonly ICoreShell _coreShell;
        private string _odbcConnectionString;

        [ImportingConstructor]
        public DbConnectionService(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public string EditConnectionString(string odbcConnectionString) {
            var originalConnectionString = (odbcConnectionString.OdbcToSqlClient()
                                                    ?? _odbcConnectionString.OdbcToSqlClient())
                                                    ?? DefaultSqlConnectionString;
            do {
                using (var dlg = new DataConnectionDialog()) {
                    DataSource.AddStandardDataSources(dlg);
                    dlg.SelectedDataSource = DataSource.SqlDataSource;
                    dlg.SelectedDataProvider = DataProvider.SqlDataProvider;
                    try {
                        dlg.ConnectionString = originalConnectionString;
                        var result = DataConnectionDialog.Show(dlg);
                        switch (result) {
                            case DialogResult.Cancel:
                                return odbcConnectionString;
                            case DialogResult.OK:
                                var sqlString = dlg.ConnectionString;
                                if (IsSqlAADConnection(sqlString)) {
                                    CheckSqlOdbcDriverVersion();
                                }
                                _odbcConnectionString = sqlString.SqlClientToOdbc();
                                break;
                        }
                        break;
                    } catch (ArgumentException) {
                        if (_coreShell.ShowMessage(Resources.Error_ConnectionStringFormat, MessageButtons.YesNo) == MessageButtons.No) {
                            break;
                        }
                        _odbcConnectionString = originalConnectionString;
                    }
                }
            } while (true);

            return _odbcConnectionString;
        }

        private bool IsSqlAADConnection(string connectionString) {
            var csb = new SqlConnectionStringBuilder(connectionString);
            return csb.Authentication == SqlAuthenticationMethod.ActiveDirectoryIntegrated;
        }

        internal bool CheckSqlOdbcDriverVersion() {
            using (var hklm = _coreShell.GetService<IRegistry>().OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                using (var odbcKey = hklm.OpenSubKey(@"SOFTWARE\ODBC\ODBCINST.INI\ODBC Driver 13 for SQL Server")) {
                    var driverPath = odbcKey.GetValue("Driver") as string;
                    if (!string.IsNullOrEmpty(driverPath)) {
                        var fs = _coreShell.FileSystem();
                        if (fs.FileExists(driverPath)) {
                            var version = fs.GetFileVersion(driverPath);
                            if (version >= new Version("2015.131.4413.46")) {
                                return true;
                            }
                        }
                    }
                }
            }
            var app = _coreShell.GetService<IApplication>();
            var link = FormatLocalizedLink(app.LocaleId, "https://www.microsoft.com/{0}/download/details.aspx?id=53339");
            _coreShell.ShowErrorMessage(Resources.Error_OdbcDriver.FormatInvariant(link));
            _coreShell.Process().Start(link);
            return false;
        }

        private static string FormatLocalizedLink(int localeId, string format) {
            var culture = CultureInfo.GetCultureInfo(localeId);
            return string.Format(CultureInfo.InvariantCulture, format, culture.Name);
        }
    }
}
