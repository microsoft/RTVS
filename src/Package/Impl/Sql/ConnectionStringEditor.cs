// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing.Design;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Application.Configuration;
using Microsoft.R.Components.Sql;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Sql {
    /// <summary>
    /// Implements visual editing of database connection strings
    /// via VS data services.
    /// </summary>
    internal sealed class ConnectionStringEditor : UITypeEditor {
        /// <summary>
        /// Category as it appears in settings.r. Example: # [Category] SQL
        /// </summary>
        public const string ConnectionStringEditorCategory = "SQL";
        /// <summary>
        /// Editor as it appears in settings.r. Example: # [Editor] ConnectionStringEditor
        /// </summary>
        public const string ConnectionStringEditorName = "ConnectionStringEditor";

        private readonly IDbConnectionService _dbcs;

        public ConnectionStringEditor() : this(null) { }
        public ConnectionStringEditor(IDbConnectionService dbcs) {
            _dbcs = dbcs ?? VsAppShell.Current.GetService<IDbConnectionService>();
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) => _dbcs.EditConnectionString(value as string);

        /// <summary>
        /// Provides type of the UI editor for database connection strings
        /// </summary>
        [Export(typeof(IConfigurationSettingUIEditorProvider))]
        [Name(ConnectionStringEditor.ConnectionStringEditorName)]
        private class EditorProvider : IConfigurationSettingUIEditorProvider {
            public Type EditorType => typeof(ConnectionStringEditor);
        }
    }
}
