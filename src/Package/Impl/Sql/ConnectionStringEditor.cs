// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing.Design;
using System.Windows.Forms;
using Microsoft.Common.Core.Shell;
using Microsoft.Data.ConnectionUI;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Sql {
    internal sealed class ConnectionStringEditor : UITypeEditor {
        private readonly ICoreShell _coreShell;

        public ConnectionStringEditor() : this(null) { }
        public ConnectionStringEditor(ICoreShell coreShell) {
            _coreShell = coreShell ?? VsAppShell.Current;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
            bool retry;
            do {
                retry = false;
                using (var dlg = new DataConnectionDialog()) {
                    DataSource.AddStandardDataSources(dlg);
                    dlg.SelectedDataSource = DataSource.SqlDataSource;
                    try {
                        dlg.ConnectionString = value as string;
                        if (DialogResult.OK == DataConnectionDialog.Show(dlg)) {
                            value = dlg.ConnectionString;
                        }
                    } catch (ArgumentException) {
                        retry = _coreShell.ShowMessage(
                                   Resources.Error_ConnectionStringFormat, MessageButtons.YesNo)
                                   == MessageButtons.Yes;
                    }
                    value = retry ? null : value;
                }
            } while (retry);

            return value;
        }

        [Export(typeof(IConfigurationSettingUIEditorProvider))]
        [Name("ConnectionStringEditor")]
        class EditorProvider : IConfigurationSettingUIEditorProvider {
            public Type EditorType => typeof(ConnectionStringEditor);
        }
    }
}
