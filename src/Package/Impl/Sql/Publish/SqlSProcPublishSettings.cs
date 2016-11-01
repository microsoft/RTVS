// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Settings;

namespace Microsoft.VisualStudio.R.Package.Sql.Publish {
    /// <summary>
    /// Represents persistent settings for the SQL stored procedure publishing dialog.
    /// </summary>
    internal class SqlSProcPublishSettings {
        public const string DefaultRCodeTableName = "RCodeTable";

        internal const string TargetTypeSettingName = "SqlSprocPublishTargetType";
        internal const string TargetDatabaseConnectionSettingName = "SqlSprocPublishTargetDatabase";
        internal const string TargetProjectSettingName = "SqlSprocPublishTargetProject";
        internal const string TableNameSettingName = "SqlSprocPublishTableName";
        internal const string CodePlacementSettingName = "SqlSprocPublishCodePlacement";
        internal const string QuoteTypeSettingName = "SqlSprocPublishQuoteType";

        /// <summary>
        /// Target SQL table name
        /// </summary>
        public string TableName { get; set; } = DefaultRCodeTableName;

        /// <summary>
        /// Target database connection name
        /// </summary>
        public string TargetDatabaseConnection { get; set; }

        /// <summary>
        /// Target database project name
        /// </summary>
        public string TargetProject { get; set; }

        /// <summary>
        /// Target database project name
        /// </summary>
        public PublishTargetType TargetType { get; set; } = PublishTargetType.Dacpac;

        /// <summary>
        /// Determines where to place R code in SQL
        /// </summary>
        public RCodePlacement CodePlacement { get; set; } = RCodePlacement.Inline;

        /// <summary>
        /// Determines type of quoting for SQL names with spaces
        /// </summary>
        public SqlQuoteType QuoteType { get; set; } = SqlQuoteType.None;

        public SqlSProcPublishSettings() { }

        public SqlSProcPublishSettings(IEditorSettingsStorage settingsStorage) {
            Load(settingsStorage);
        }

        private void Load(IEditorSettingsStorage settingsStorage) {
            TargetType = (PublishTargetType)settingsStorage.GetInteger(TargetTypeSettingName, (int)PublishTargetType.Dacpac);
            TargetDatabaseConnection = settingsStorage.GetString(TargetDatabaseConnectionSettingName, string.Empty);
            TargetProject = settingsStorage.GetString(TargetProjectSettingName, string.Empty);
            TableName = settingsStorage.GetString(TableNameSettingName, SqlSProcPublishSettings.DefaultRCodeTableName);
            CodePlacement = (RCodePlacement)settingsStorage.GetInteger(CodePlacementSettingName, (int)RCodePlacement.Inline);
            QuoteType = (SqlQuoteType)settingsStorage.GetInteger(QuoteTypeSettingName, (int)SqlQuoteType.None);
        }

        public void Save(IWritableEditorSettingsStorage settingsStorage) {
            settingsStorage.SetInteger(TargetTypeSettingName,(int)TargetType);
            settingsStorage.SetString(TargetDatabaseConnectionSettingName, TargetDatabaseConnection);
            settingsStorage.SetString(TargetProjectSettingName, TargetProject);
            settingsStorage.SetString(TableNameSettingName, TableName);
            settingsStorage.SetInteger(CodePlacementSettingName, (int)CodePlacement);
            settingsStorage.SetInteger(QuoteTypeSettingName, (int)QuoteType);
        }
    }
}