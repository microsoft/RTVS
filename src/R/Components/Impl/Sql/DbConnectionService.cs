// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Microsoft.Common.Core.Shell;
using Microsoft.Data.ConnectionUI;

namespace Microsoft.R.Components.Sql {
    [Export(typeof(IDbConnectionService))]
    internal sealed class DbConnectionService : IDbConnectionService {
        private const string _defaultSqlConnectionString = "Data Source=(local);Integrated Security=true";
        private readonly ICoreShell _coreShell;
        private string _odbcConnectionString;

        [ImportingConstructor]
        public DbConnectionService(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

        public string EditConnectionString(string odbcConnectionString) {
            do {
                using (var dlg = new DataConnectionDialog()) {
                    DataSource.AddStandardDataSources(dlg);
                    dlg.SelectedDataSource = DataSource.SqlDataSource;
                    try {
                        dlg.ConnectionString = (odbcConnectionString.OdbcToSqlClient() 
                                                    ?? _odbcConnectionString.OdbcToSqlClient()) 
                                                        ?? _defaultSqlConnectionString;
                        var result = DataConnectionDialog.Show(dlg);
                        switch(result) {
                            case DialogResult.Cancel:
                                return null;
                            case DialogResult.OK:
                                _odbcConnectionString = dlg.ConnectionString.SqlClientToOdbc();
                                break;
                        }
                        break;
                    } catch (ArgumentException) {
                        if (_coreShell.ShowMessage(Resources.Error_ConnectionStringFormat, MessageButtons.YesNo) == MessageButtons.No) {
                            break;
                        }
                        _odbcConnectionString = _defaultSqlConnectionString.SqlClientToOdbc();
                    }
                }
            } while (true);

            return _odbcConnectionString;
        }
    }
}
