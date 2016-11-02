// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Shell;

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

        public SqlSProcPublishSettings(ISettingsStorage settingsStorage) {
            Load(settingsStorage);
        }

        private void Load(ISettingsStorage settingsStorage) {
            TargetType = settingsStorage.GetSetting(TargetTypeSettingName, PublishTargetType.Dacpac);
            TargetDatabaseConnection = settingsStorage.GetSetting(TargetDatabaseConnectionSettingName, string.Empty);
            TargetProject = settingsStorage.GetSetting(TargetProjectSettingName, string.Empty);
            TableName = settingsStorage.GetSetting(TableNameSettingName, DefaultRCodeTableName);
            CodePlacement = settingsStorage.GetSetting(CodePlacementSettingName, RCodePlacement.Inline);
            QuoteType = settingsStorage.GetSetting(QuoteTypeSettingName, SqlQuoteType.None);
        }

        public void Save(ISettingsStorage settingsStorage) {
            settingsStorage.SetSetting(TargetTypeSettingName, TargetType);
            settingsStorage.SetSetting(TargetDatabaseConnectionSettingName, TargetDatabaseConnection);
            settingsStorage.SetSetting(TargetProjectSettingName, TargetProject);
            settingsStorage.SetSetting(TableNameSettingName, TableName);
            settingsStorage.SetSetting(CodePlacementSettingName, CodePlacement);
            settingsStorage.SetSetting(QuoteTypeSettingName, QuoteType);
        }
    }
}